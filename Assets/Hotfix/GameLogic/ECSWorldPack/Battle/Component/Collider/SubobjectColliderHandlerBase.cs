using cfg;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum HitType
    {
        Entity,
        Obstacle,
        EntityPart,
    }

    public struct HitInfo
    {
        public long ownerEntityID;
        public long hitEntityID;
        public Vector3 hitPos;
        public Vector3 hitDir;
        public HitType hitType;
    }

    //支持碰撞间隔，支持范围碰撞，支持单个物体多次碰撞，支持最大碰撞次数
    public class SubobjectColliderHandlerBase : ColliderHandler
    {
        private Subobject _subobject;
        private Dictionary<long, int> _hitCount; //碰撞者id 次数
        private int _currentHitCount; //当前碰撞次数
        private float _hitIntervalTimer; //碰撞计时器


        protected bool HitWithLife => _subobject.HitWithLife;
        protected int MaxHitCount => _subobject.MaxHitCount;
        protected float HitInterval => _subobject.HitInterval;
        protected int SingleHitCount => _subobject.SingleHitCount;



        protected bool IsHitObstacle; //是否碰撞阻挡物
        protected bool IsHitObstacleCounted; //碰撞阻挡物是否计算到次数中
        protected bool IgnoreHitSelf; //是否忽略碰撞自己


        public virtual void Init(Subobject subObjCfg)
        {
            _subobject = subObjCfg;
            _collisionType = subObjCfg.CollisionType;
            InitMaxHits(99);


            _hitCount = new Dictionary<long, int>();
            _currentHitCount = 0;
            _hitIntervalTimer = 0;

            IsHitObstacle = true;
            IsHitObstacleCounted = true;
            IgnoreHitSelf = false;
        }


        protected virtual bool IsActiveAsSource(LogicEntity ownerEntity)
        {
            if (!ownerEntity.hasComID)
                return false;

            if (ownerEntity.hasComDeath)
                return false;

            if (_hitIntervalTimer < HitInterval && _hitIntervalTimer != 0)
                return false;

            if (_currentHitCount >= MaxHitCount)
                return false;

            return true;
        }

        public override bool CheckRawHits(LogicEntity ownerEntity, float dt)
        {
            //主动碰撞间隔
            if (HitInterval > 0)
            {
                if (_hitIntervalTimer < HitInterval)
                    _hitIntervalTimer += dt;

                if (_hitIntervalTimer < HitInterval)
                    return false;

                _hitIntervalTimer = 0;
            }

            if (!IsActiveAsSource(ownerEntity))
                return false;

            return base.CheckRawHits(ownerEntity, dt);
        }


        public override void HandleRawHits(LogicEntity ownerEntity, float dt)
        {
            foreach (var rawHit in _rawHits)
            {
                var hitCollider = rawHit.Collider;
                var hitPoint = rawHit.Point;
                if (hitCollider == null)
                    continue;

                if (!IsActiveAsSource(ownerEntity))
                    return;

                if (IsHitObstacle && hitCollider.tag == "Obstacle")
                {
                    HandleHitObstacle(ownerEntity, hitPoint);
                    continue;
                }

                var hitEntity = Main.WorldService.GetWorld().GetEntitiesWithComUnityObjectRelated(hitCollider.GetInstanceID());
                if (!Check2EntityHitable(ownerEntity, hitEntity))
                    continue;

                //处理 A碰B
                HandleHitEntity(ownerEntity, hitEntity, hitPoint, hitEntity.comUnityObjectRelated.GetGameObjectType(hitCollider.GetInstanceID()) == GameObjectType.Part);

                //处理 B碰A
                if (hitEntity.hasComCollider && !IgnoreHitSelf)
                {
                    var handler = hitEntity.comCollider.handler as SubobjectColliderHandlerBase;
                    if (handler != null && handler.IsActiveAsSource(hitEntity))
                    {
                        handler.HandleHitEntity(hitEntity, ownerEntity, hitPoint, false);
                    }
                }
            }
        }


        protected virtual void HandleHitEntity(LogicEntity ownerEntity, LogicEntity hitEntity, Vector2 hitPoint, bool isPart)
        {
            if (!Check2EntityHitable(ownerEntity, hitEntity))
            {
                return;
            }

            HitInfo hitInfo = new HitInfo();
            hitInfo.hitPos = hitPoint;
            hitInfo.hitDir = hitEntity.comTransform.position - ownerEntity.comTransform.position;
            hitInfo.hitEntityID = hitEntity.comID.id;
            hitInfo.hitType = isPart ? HitType.EntityPart : HitType.Entity;
            ProcessHitInfo(ownerEntity, hitInfo, hitEntity);
            if (HitWithLife)
            {
                CheckDestroyOwnerEntity(ownerEntity);
            }
        }

        protected virtual void HandleHitObstacle(LogicEntity ownerEntity, Vector2 hitPoint)
        {
            HitInfo hitInfo = new HitInfo();
            hitInfo.hitEntityID = -1;
            hitInfo.hitPos = hitPoint;
            hitInfo.hitDir = ownerEntity.comTransform.rotation * Vector3.right;
            hitInfo.hitType = HitType.Obstacle;
            ProcessHitInfo(ownerEntity, hitInfo);
            if (HitWithLife)
            {
                CheckDestroyOwnerEntity(ownerEntity);
            }
        }

        protected virtual void ProcessHitInfo(LogicEntity ownerEntity, HitInfo hitInfo, LogicEntity hitEntity = null)
        {
            var hitEntityID = hitInfo.hitEntityID;
            if (hitEntityID >= 0)
            {
                if (_hitCount.ContainsKey(hitEntityID))
                {
                    var hitCount = _hitCount[hitEntityID];
                    if (hitEntityID > SingleHitCount)
                        return;

                    _hitCount[hitEntityID]++;
                }
                else
                {
                    _hitCount.Add(hitEntityID, 1);
                }

                _currentHitCount++;
            }

            if (hitEntityID == -1 && IsHitObstacle && IsHitObstacleCounted)
            {
                _currentHitCount++;
            }

            if (ownerEntity.hasComSubobject)
            {
                ProcessSubobjectHit(ownerEntity, hitInfo, hitEntity, hitEntityID);
            }
        }

        protected virtual void ProcessSubobjectHit(LogicEntity ownerEntity, HitInfo hitInfo, LogicEntity hitEntity, long hitEntityID)
        {
            var comSubobject = ownerEntity.comSubobject;
            hitInfo.ownerEntityID = comSubobject.OwnerId;
            if (hitEntityID >= 0)
            {
                if (comSubobject.Agent != null)
                {
                    comSubobject.Agent.Trigger(BTAction.OnHitEntity, hitInfo);
                }
            }
            else
            {
                if (comSubobject.Agent != null)
                {
                    comSubobject.Agent.Trigger(BTAction.OnHitObstacle, hitInfo);
                }
            }
        }


        protected virtual void CheckDestroyOwnerEntity(LogicEntity ownerEntity)
        {
            if (_currentHitCount >= MaxHitCount)
            {
                ownerEntity.ReplaceComLife(0);
            }
        }




        protected virtual bool Check2EntityHitable(LogicEntity ownerEntity, LogicEntity hitEntity)
        {
            if (ownerEntity == null)
                return false;
            if (hitEntity == null)
                return false;

            if (!ownerEntity.hasComID)
                return false;

            if (!ownerEntity.hasComTransform)
                return false;

            if (!ownerEntity.hasComUnityObjectRelated)
                return false;

            if (!ownerEntity.hasComFaction)
                return false;

            if (ownerEntity.hasComDeath)
                return false;

            if (!hitEntity.hasComID)
                return false;

            if (!hitEntity.hasComTransform)
                return false;

            if (!hitEntity.hasComUnityObjectRelated)
                return false;

            if (!hitEntity.hasComFaction)
                return false;

            if (hitEntity.hasComDeath)
                return false;

            if (ownerEntity.hasComSubobject)
            {
                var comSubobj = ownerEntity.comSubobject;
                //排除主人
                if (hitEntity.comID.id == comSubobj.OwnerId)
                {
                    return false;
                }

                //排除碰撞白名单
                var list = ownerEntity.comSubobject.Agent.Context.GetObject<List<int>>(KVType.HitWhiteList);
                foreach (var item in list)
                {
                    if (item == hitEntity.comID.id)
                    {
                        return false;
                    }
                }
            }

            //排除自己
            if (hitEntity.comID.id == ownerEntity.comID.id)
            {
                return false;
            }

            //排除友方 
            if (hitEntity.comFaction.faction == ownerEntity.comFaction.faction)
            {
                return false;
            }

            if (hitEntity.hasComProperty)
            {
                //排除不能被碰撞的
                if (!hitEntity.comProperty.isBlockable)
                {
                    return false;
                }

                //排除死亡的
                if (!hitEntity.comProperty.isAlive && hitEntity.comProperty.isDieable)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _subobject = null;

            _hitCount = new Dictionary<long, int>();
            _currentHitCount = 0;
            _hitIntervalTimer = 0;

            IsHitObstacle = true;
            IsHitObstacleCounted = true;
            IgnoreHitSelf = false;
        }
    }
}
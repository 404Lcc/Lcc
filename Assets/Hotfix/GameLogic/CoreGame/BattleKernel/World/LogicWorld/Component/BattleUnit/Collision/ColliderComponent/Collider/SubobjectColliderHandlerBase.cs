using System.Collections.Generic;
using System.Linq;
using PBConfig;
using Vector3 = UnityEngine.Vector3;

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
        public long selfEntityId; //自身entityId
        public long hitEntityID;
        public Vector3 hitPos;
        public Vector3 hitDir;
        public Vector3 hitNormal;
        public HitType hitType;
        public bool keepAliveOnHit;
    }

    //支持碰撞间隔，支持范围碰撞，支持单个物体多次碰撞，支持最大碰撞次数
    public class SubobjectColliderHandlerBase : ColliderHandler
    {
        private TSubobject _subobjectCfg;
        private Dictionary<long, int> _hitCount = new Dictionary<long, int>(); //碰撞者id 次数
        private int _currentHitCount; //当前碰撞次数
        internal float _hitIntervalTimer; //碰撞计时器


        protected bool HitWithLife => _subobjectCfg.HitWithLife;
        public int MaxHitCount { get; set; }
        public int CurrentHitCount => _currentHitCount;
        public int RemainingHitCount => MaxHitCount > _currentHitCount ? MaxHitCount - _currentHitCount : 0;
        protected float HitInterval => _subobjectCfg.HitInterval;
        protected int SingleHitCount => _subobjectCfg.SingleHitCount;

        private float mHitIntervalReduce = 0;

        protected bool IsHitObstacle; //是否碰撞阻挡物
        protected bool IsHitObstacleCounted; //碰撞阻挡物是否计算到次数中
        protected bool IgnoreHitSelf; //是否忽略碰撞自己

        /// <summary>
        /// 本次命中后是否应销毁子物体（穿透剩余次数 > 0 时不销毁）
        /// </summary>
        public static bool ShouldDestroySubobjectAfterHit(LogicEntity subobjectEntity)
        {
            if (subobjectEntity == null || !subobjectEntity.hasComCollider)
                return true;
            if (subobjectEntity.comCollider.handler is SubobjectColliderHandlerBase handler)
                return handler.RemainingHitCount <= 0;
            return true;
        }

        public static bool IsInHitWhiteList(LogicEntity subobjectEntity, long entityId)
        {
            if (subobjectEntity == null || !subobjectEntity.hasComSubobject)
                return false;
            var varEnv = subobjectEntity.comSubobject.Logic?.VarEnvRef;
            if (varEnv == null || !varEnv.HasVar<List<int>>(CvKey.CV_HitWhiteList))
                return false;
            varEnv.ReadVar(CvKey.CV_HitWhiteList, out List<int> list);
            if (list == null)
                return false;
            foreach (var id in list)
            {
                if (id == entityId)
                    return true;
            }

            return false;
        }

        public static void AddHitWhiteListEntry(LogicEntity subobjectEntity, long entityId)
        {
            if (subobjectEntity == null || !subobjectEntity.hasComSubobject)
                return;
            var logic = subobjectEntity.comSubobject.Logic;
            if (logic == null)
                return;

            List<int> list;
            if (logic.VarEnvRef.HasVar<List<int>>(CvKey.CV_HitWhiteList))
                logic.VarEnvRef.ReadVar(CvKey.CV_HitWhiteList, out list);
            else
                list = new List<int>();

            foreach (var id in list)
            {
                if (id == entityId)
                    return;
            }

            list.Add((int)entityId);
            logic.VarEnvRef.WriteVar(CvKey.CV_HitWhiteList, list);
        }

        public void SetObstacleCounted(bool counted)
        {
            IsHitObstacleCounted = counted;
        }


        /// <summary>
        /// 子物体碰撞器初始化
        /// </summary>
        /// <param name="subObjCfg"></param>
        public virtual void Init(TSubobject subObjCfg)
        {
            _subobjectCfg = subObjCfg;

            MaxHitCount = _subobjectCfg.MaxHitCount;

            // 复用字典，保留桶容量。
            _hitCount.Clear();
            _currentHitCount = 0;
            _hitIntervalTimer = 0;
            mHitIntervalReduce = 0;

            IsHitObstacle = true;
            IsHitObstacleCounted = true;
            IgnoreHitSelf = false;
        }


        /// <summary>
        /// 判断自身符合碰撞条件
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <returns></returns>
        public override bool IsActiveAsSource(LogicEntity selfEntity)
        {
            if (!selfEntity.hasComID)
                return false;

            if (selfEntity.hasComDeath)
                return false;

            if (CheckMaxHitCountAsSource())
                return false;

            if (_hitIntervalTimer > 0)
                return false;

            return true;
        }

        /// <summary>
        /// 检测自身碰撞次数是否达到最大
        /// </summary>
        protected virtual bool CheckMaxHitCountAsSource()
        {
            if (_currentHitCount >= MaxHitCount)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断两个entity满足碰撞条件
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="hitEntity"></param>
        /// <returns></returns>
        protected virtual bool Check2EntityHitActive(LogicEntity selfEntity, LogicEntity hitEntity)
        {
            if (selfEntity == null)
                return false;

            if (hitEntity == null)
                return false;

            if (!selfEntity.hasComID)
                return false;

            if (!selfEntity.hasComTransform)
                return false;

            if (!selfEntity.hasComFaction)
                return false;

            if (selfEntity.hasComDeath)
                return false;

            if (!hitEntity.hasComID)
                return false;

            if (!hitEntity.hasComTransform)
                return false;

            if (!hitEntity.hasComFaction)
                return false;

            if (hitEntity.hasComDeath)
                return false;

            if (!hitEntity.hasComAttributes)
                return false;

            if (selfEntity.hasComSubobject)
            {
                var comSubobj = selfEntity.comSubobject;
                //排除主人
                if (hitEntity.ID == comSubobj.SpellEntityId)
                {
                    return false;
                }

                //排除碰撞白名单
                if (IsInHitWhiteList(selfEntity, hitEntity.ID))
                    return false;

                //自己作为子物体不能碰隐形
                if (!comSubobj.Cfg.HitCloak && hitEntity.IsCloaked())
                {
                    return false;
                }
            }

            //排除自己
            if (hitEntity == selfEntity)
            {
                return false;
            }

            //排除友方 
            if (hitEntity.comFaction.Faction == selfEntity.comFaction.Faction)
            {
                return false;
            }

            //排除不能被碰撞的
            if (!hitEntity.comAttributes.GetValue<bool>(AttributeBool.CanCollision, true))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生成生数据
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override bool CheckRawHits(LogicEntity selfEntity, float dt)
        {
            //主动碰撞间隔
            if (_hitIntervalTimer > 0)
            {
                _hitIntervalTimer -= dt;
            }

            if (!IsActiveAsSource(selfEntity))
                return false;

            return base.CheckRawHits(selfEntity, dt);
        }


        /// <summary>
        /// 处理生数据 触发碰撞
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="rawHits"></param>
        /// <param name="dt"></param>
        public override void HandleRawHits(LogicEntity selfEntity, RawHit[] rawHits, float dt)
        {
            foreach (var rawHit in rawHits)
            {
                var hitEntity = rawHit.HitEntity;
                var hitPoint = rawHit.Point;
                if (IsHitObstacle && rawHit.ColliderLayer == UnityPhysicsHitMaker.BlockLayer)
                {
                    HandleHitObstacle(selfEntity, rawHit);
                    continue;
                }

                if (hitEntity == null)
                    continue;

                if (!Check2EntityHitActive(selfEntity, hitEntity))
                    continue;

                //处理 A碰B
                HandleHitEntity(selfEntity, hitEntity, rawHit);

                //处理 B碰A
                if (hitEntity.hasComCollider && !IgnoreHitSelf)
                {
                    var handler = hitEntity.comCollider.handler;
                    if (handler == null)
                    {
                        continue;
                    }

                    if (handler.IsActiveAsSource(hitEntity))
                    {
                        var otherHit = rawHit;
                        otherHit.Normal *= -1f;
                        otherHit.HitEntity = selfEntity;
                        handler.HandleHitEntity(hitEntity, selfEntity, otherHit);
                    }
                }
            }
        }

        /// <summary>
        /// 碰撞其他entity
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="hitEntity"></param>
        /// <param name="hitPoint"></param>
        /// <param name="isPart"></param>
        public override void HandleHitEntity(LogicEntity selfEntity, LogicEntity hitEntity, RawHit hit)
        {
            if (!Check2EntityHitActive(selfEntity, hitEntity))
            {
                return;
            }

            _hitIntervalTimer = HitInterval - mHitIntervalReduce;
            if (BattleLogger.IsDebugEnabled)
                BattleLogger.LogDebug($"[碰撞冷却] selfEntity={selfEntity.ID}, hitEntity={hitEntity.ID}, 碰撞间隔={HitInterval}s, 间隔减少={mHitIntervalReduce}s, 实际冷却={_hitIntervalTimer:F3}s, 当前碰撞次数={_currentHitCount}/{MaxHitCount}");

            HitInfo hitInfo = new HitInfo();
            hitInfo.hitPos = hit.Point;
            hitInfo.hitDir = hit.Normal == Vector3.zero ? hitEntity.comTransform.position - selfEntity.comTransform.position : hit.Normal;
            hitInfo.hitNormal = hit.Normal;
            hitInfo.hitEntityID = hitEntity.ID;
            hitInfo.hitType = HitType.Entity;
            if (!TryAddHitCount(hitInfo))
                return;
            if (selfEntity.hasComSubobject)
            {
                SendHitCommand(selfEntity, hitInfo);
            }
        }

        /// <summary>
        /// 碰撞障碍物
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="hit"></param>
        protected virtual void HandleHitObstacle(LogicEntity selfEntity, RawHit hit)
        {
            HitInfo hitInfo = new HitInfo();
            hitInfo.hitEntityID = hit.HitEntity?.ID ?? -1;
            hitInfo.hitPos = hit.Point;
            hitInfo.hitDir = selfEntity.comTransform.rotation * Vector3.forward;
            hitInfo.hitNormal = hit.Normal;
            hitInfo.hitType = HitType.Obstacle;
            _hitIntervalTimer = HitInterval - mHitIntervalReduce;
            if (selfEntity.hasComSubobject)
            {
                SendHitCommand(selfEntity, hitInfo, 0);
            }
        }

        /// <summary>
        /// 记录碰撞次数
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="hitInfo"></param>
        /// <param name="hitEntity"></param>
        protected virtual bool TryAddHitCount(HitInfo hitInfo)
        {
            var hitEntityID = hitInfo.hitEntityID;
            if (_currentHitCount >= MaxHitCount)
            {
                return false;
            }

            if (hitEntityID >= 0)
            {
                if (_hitCount.ContainsKey(hitEntityID))
                {
                    var hitCount = _hitCount[hitEntityID];
                    if (hitCount >= SingleHitCount)
                        return false;

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

            return true;
        }

        public int GetHitCount(long hitEntityID)
        {
            return _hitCount.ContainsKey(hitEntityID) ? _hitCount[hitEntityID] : 0;
        }

        public List<long> GetAlreadyHitList()
        {
            return _hitCount.Keys.ToList();
        }

        public void ClearHitCount(long hitEntityID)
        {
            if (_hitCount.ContainsKey(hitEntityID))
            {
                _hitCount.Remove(hitEntityID);
            }
        }

        public void ClearHitCountExcept(long hitEntityID)
        {
            foreach (var key in _hitCount.Keys.ToList())
            {
                if (key != hitEntityID)
                {
                    _hitCount.Remove(key);
                }
            }
        }

        public void ClearAllHitCount()
        {
            _hitCount.Clear();
        }

        public void ResetHitState()
        {
            _hitCount.Clear();
            _currentHitCount = 0;
            _hitIntervalTimer = 0f;
        }

        public void ReduceHitInterval(float reduceNum)
        {
            mHitIntervalReduce += reduceNum;
        }

        public void ReduceHitIntervalPercent(float percent)
        {
            mHitIntervalReduce += HitInterval * percent;
        }

        public void ResetHitIntervalTimer()
        {
            _hitIntervalTimer = 0f;
        }


        /// <summary>
        /// 发送碰撞命令
        /// </summary>
        /// <param name="selfEntity"></param>
        /// <param name="hitInfo"></param>
        /// <param name="hitEntity"></param>
        /// <param name="hitEntityID"></param>
        protected void SendHitCommand(LogicEntity selfEntity, HitInfo hitInfo, int? hitLeftCountOverride = null)
        {
            var comSubobject = selfEntity.comSubobject;
            hitInfo.selfEntityId = selfEntity.ID;
            var cmd = new EntityCommand()
            {
                CmdType = EntityCmdType.Nt_ColliderHit,
                EntityID = selfEntity.ID,
                V0 = HitWithLife,
                V1 = hitLeftCountOverride ?? MaxHitCount - _currentHitCount,
                HitInfo = hitInfo,
            };
            selfEntity.SendCmd(cmd);
        }

        /// <summary>
        /// 回收
        /// </summary>
        public override void OnRecycle()
        {
            base.OnRecycle();

            _subobjectCfg = null;

            // 复用字典，保留桶容量。
            _hitCount.Clear();
            _currentHitCount = 0;
            _hitIntervalTimer = 0;

            IsHitObstacle = true;
            IsHitObstacleCounted = true;
            IgnoreHitSelf = false;
        }
    }
}
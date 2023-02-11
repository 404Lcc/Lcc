using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SkillExecution : Entity, IAbilityExecution, IUpdate
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }



        public SkillAbility SkillAbility => (SkillAbility)AbilityEntity;

        public ExecutionConfigObject executionConfigObject;
        public List<CombatEntity> inputSkillTargetList = new List<CombatEntity>();
        public CombatEntity inputTarget;
        public Vector3 inputPoint;
        public float inputDirection;
        public long originTime;


        public bool actionOccupy = true;


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);
            AbilityEntity = p1 as SkillAbility;

            originTime = Time.Instance.ClientNow();
        }


        public void LoadExecutionEffect()
        {
            AddComponent<ExecutionEffectComponent>();
        }
        public void Update()
        {
            var nowSeconds = (double)(Time.Instance.ClientNow() - originTime) / 1000;

            if (nowSeconds >= executionConfigObject.TotalTime)
            {
                EndExecute();
            }
        }


        public void BeginExecute()
        {
            GetParent<CombatEntity>().spellingSkillExecution = this;
            if (SkillAbility != null)
            {
                SkillAbility.spelling = true;
            }

            GetComponent<ExecutionEffectComponent>().BeginExecute();

            FireEvent(nameof(BeginExecute));
        }

        public void EndExecute()
        {
            GetParent<CombatEntity>().spellingSkillExecution = null;
            if (SkillAbility != null)
            {
                SkillAbility.spelling = false;
            }
            inputSkillTargetList.Clear();
            Dispose();

        }


        public void SpawnCollisionItem(ExecuteClipData clipData)
        {
            //在当前战斗上下文创建创生体
            var abilityItem = Root.Instance.Scene.AddChildren<AbilityItem, SkillExecution>(this); //Entity.Create<AbilityItem>(this);
            abilityItem.AddComponent<AbilityItemCollisionExecuteComponent, ExecuteClipData>(clipData);

            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.PathFly) PathFlyProcess(abilityItem);
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.SelectedDirectionPathFly) PathFlyProcess(abilityItem);
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.TargetFly) TargetFlyProcess(abilityItem);
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.ForwardFly) ForwardFlyProcess(abilityItem);
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.SelectedPosition) FixedPositionProcess(abilityItem);
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.SelectedDirection) FixedDirectionProcess(abilityItem);

            CreateAbilityItemProxy(abilityItem);
        }

        /// <summary>
        /// 目标飞行碰撞体
        /// </summary>
        /// <param name="abilityItem"></param>
        private void TargetFlyProcess(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.targetEntity = inputTarget;
            abilityItem.Position = OwnerEntity.Position;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveToWithTime(inputTarget, clipData.Duration);
        }

        /// <summary>
        /// 前向飞行碰撞体
        /// </summary>
        /// <param name="abilityItem"></param>
        private void ForwardFlyProcess(AbilityItem abilityItem)
        {
            abilityItem.Position = OwnerEntity.Position;
            var x = Mathf.Sin(Mathf.Deg2Rad * inputDirection);
            var z = Mathf.Cos(Mathf.Deg2Rad * inputDirection);
            var destination = abilityItem.Position + new Vector3(x, 0, z) * 30;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveTo(destination, 1f).OnMoveFinish(() =>
            {
                abilityItem.Dispose();
            });
        }

        /// <summary>
        /// 路径飞行
        /// </summary>
        /// <param name="abilityItem"></param>
        private void PathFlyProcess(AbilityItem abilityItem)
        {
            abilityItem.Position = OwnerEntity.Position;
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            var tempPoints = clipData.CollisionExecuteData.GetCtrlPoints();

            var angle = OwnerEntity.Rotation.eulerAngles.y - 90;
            var a = angle * MathF.PI / 180;


            abilityItem.Position = tempPoints[0].position;
            var moveComp = abilityItem.AddComponent<AbilityItemBezierMoveComponent>();
            moveComp.positionEntity = abilityItem;
            moveComp.ctrlPointList = tempPoints;
            moveComp.originPosition = OwnerEntity.Position;
            moveComp.rotateAgree = a;
            moveComp.speed = clipData.Duration / 10;
            moveComp.DOMove();
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }

        /// <summary>
        /// 固定位置碰撞体
        /// </summary>
        /// <param name="abilityItem"></param>
        private void FixedPositionProcess(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.Position = inputPoint;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }

        /// <summary>
        /// 固定方向碰撞体
        /// </summary>
        /// <param name="abilityItem"></param>
        private void FixedDirectionProcess(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.Position = OwnerEntity.Position;
            abilityItem.Rotation = OwnerEntity.Rotation;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }

        /// <summary>
        /// 创建技能碰撞体
        /// </summary>
        /// <param name="abilityItem"></param>
        /// <returns></returns>
        private GameObject CreateAbilityItemProxy(AbilityItem abilityItem)
        {
            var proxyObj = new GameObject("AbilityItemProxy");
            proxyObj.transform.position = abilityItem.Position;
            proxyObj.transform.rotation = abilityItem.Rotation;
            proxyObj.AddComponent<AbilityItemProxy>().abilityItem = abilityItem;
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().CollisionExecuteData;

            if (clipData.Shape == CollisionShape.Sphere)
            {
                proxyObj.AddComponent<SphereCollider>().enabled = false;
                proxyObj.GetComponent<SphereCollider>().radius = clipData.Radius;
            }
            if (clipData.Shape == CollisionShape.Box)
            {
                proxyObj.AddComponent<BoxCollider>().enabled = false;
                proxyObj.GetComponent<BoxCollider>().center = clipData.Center;
                proxyObj.GetComponent<BoxCollider>().size = clipData.Size;
            }

            proxyObj.AddComponent<OnTriggerEnterCallback>().triggerEnterAction = (other) =>
            {
                //var combatEntity = CombatContext.Instance.Object2Entities[other.gameObject];
                //abilityItem.OnCollision(combatEntity);
            };

            proxyObj.GetComponent<Collider>().enabled = true;

            if (clipData.ObjAsset != null)
            {
                var effectObj = GameObject.Instantiate(clipData.ObjAsset, proxyObj.transform);
                effectObj.transform.localPosition = Vector3.zero;
                effectObj.transform.localRotation = Quaternion.identity;
            }

            return proxyObj;
        }
    }
}
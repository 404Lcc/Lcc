using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SkillExecution : Entity, IAbilityExecution, IUpdate
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity => GetParent<CombatEntity>();



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
            //在当前战斗上下文创建创生体 todo
            var abilityItem = Root.Instance.Scene.AddChildren<AbilityItem, SkillExecution>(this);
            abilityItem.AddComponent<AbilityItemCollisionExecuteComponent, ExecuteClipData>(clipData);

            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.FixedPosition)
            {
                FixedPositionItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.FixedDirection)
            {
                FixedDirectionItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.TargetFly)
            {
                TargetFlyItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.ForwardFly)
            {
                ForwardFlyItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.PathFly)
            {
                PathFlyItem(abilityItem);
            }


            CreateAbilityItemProxy(abilityItem);
        }

        private void TargetFlyItem(AbilityItem abilityItem)
        {
            abilityItem.Position = OwnerEntity.Position;
            ExecuteClipData clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveToWithTime(inputTarget, clipData.Duration);
        }

        private void ForwardFlyItem(AbilityItem abilityItem)
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

        private void PathFlyItem(AbilityItem abilityItem)
        {
            abilityItem.Position = OwnerEntity.Position;
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            var pointList = clipData.CollisionExecuteData.GetPointList();

            var angle = OwnerEntity.Rotation.eulerAngles.y - 90;

            abilityItem.Position = pointList[0].position;
            var moveComp = abilityItem.AddComponent<AbilityItemBezierMoveComponent>();
            moveComp.positionEntity = abilityItem;
            moveComp.pointList = pointList;
            moveComp.originPosition = OwnerEntity.Position;
            moveComp.rotateAgree = angle * MathF.PI / 180;
            moveComp.speed = clipData.Duration / 10;
            moveComp.DOMove();
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }

        private void FixedPositionItem(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.Position = inputPoint;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }


        private void FixedDirectionItem(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.Position = OwnerEntity.Position;
            abilityItem.Rotation = OwnerEntity.Rotation;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }


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
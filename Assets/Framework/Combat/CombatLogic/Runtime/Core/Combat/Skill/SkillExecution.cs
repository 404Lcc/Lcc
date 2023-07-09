using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SkillExecution : Entity, IAbilityExecution
    {
        public Entity Ability { get; set; }
        public Combat Owner => GetParent<Combat>();



        public SkillAbility SkillAbility => (SkillAbility)Ability;

        public ExecutionConfigObject executionConfigObject;

        public List<Combat> targetList = new List<Combat>();

        public Vector3 inputPoint;
        public float inputDirection;


        public bool actionOccupy = true;


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);
            Ability = p1 as SkillAbility;


        }


        public void LoadExecutionEffect()
        {
            AddComponent<ExecutionEffectComponent>();
            Timer.Instance.NewOnceTimer((long)(executionConfigObject.TotalTime * 1000), EndExecute);
        }


        public void BeginExecute()
        {
            GetParent<Combat>().spellingSkillExecution = this;
            if (SkillAbility != null)
            {
                SkillAbility.spelling = true;
            }

            GetComponent<ExecutionEffectComponent>().BeginExecute();
        }

        public void EndExecute()
        {
            targetList.Clear();

            GetParent<Combat>().spellingSkillExecution = null;
            if (SkillAbility != null)
            {
                SkillAbility.spelling = false;
            }
            Dispose();

        }


        public void SpawnCollisionItem(ExecuteClipData clipData)
        {
            var abilityItem = CombatContext.Instance.AddAbilityItem(this, clipData);

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
        }

        private void TargetFlyItem(AbilityItem abilityItem)
        {
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            ExecuteClipData clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveToWithTime(targetList[0].TransformComponent, clipData.Duration);
        }

        private void ForwardFlyItem(AbilityItem abilityItem)
        {
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            var x = Mathf.Sin(Mathf.Deg2Rad * inputDirection);
            var z = Mathf.Cos(Mathf.Deg2Rad * inputDirection);
            var destination = abilityItem.TransformComponent.position + new Vector3(x, 0, z) * 30;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveTo(destination, 1f).OnMoveFinish(() =>
            {
                abilityItem.Dispose();
            });
        }

        private void PathFlyItem(AbilityItem abilityItem)
        {
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            var pointList = clipData.CollisionExecuteData.GetPointList();

            var angle = Owner.TransformComponent.rotation.eulerAngles.y - 90;

            abilityItem.TransformComponent.position = pointList[0].position;
            var moveComp = abilityItem.AddComponent<AbilityItemBezierMoveComponent>();
            moveComp.abilityItem = abilityItem;
            moveComp.pointList = pointList;
            moveComp.originPosition = Owner.TransformComponent.position;
            moveComp.rotateAgree = angle * MathF.PI / 180;
            moveComp.speed = clipData.Duration / 10;
            moveComp.DOMove();
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, long>((long)(clipData.Duration * 1000));
        }

        private void FixedPositionItem(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.TransformComponent.position = inputPoint;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, long>((long)(clipData.Duration * 1000));
        }


        private void FixedDirectionItem(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            abilityItem.TransformComponent.rotation = Owner.TransformComponent.rotation;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, long>((long)(clipData.Duration * 1000));
        }
    }
}
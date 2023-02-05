using UnityEngine;

namespace LccModel
{
    public class AbilityItem : Entity, IPosition
    {
        public IAbilityExecution abilityExecution;
        public Entity abilityEntity;

        public EffectApplyType effectApplyType;
        public CombatEntity targetEntity;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            abilityExecution = p1 as IAbilityExecution;//技能执行体

            abilityEntity = abilityExecution.AbilityEntity;

            if (abilityEntity == null)
            {
                return;
            }
            var abilityEffects = abilityEntity.GetComponent<AbilityEffectComponent>().abilityEffectList;
            foreach (var abilityEffect in abilityEffects)
            {
                if (abilityEffect.effectConfig.DecoratorList != null)
                {
                    foreach (var effectDecorator in abilityEffect.effectConfig.DecoratorList)
                    {
                        if (effectDecorator is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
                        {
                            AddComponent<AbilityItemTargetCounterComponent>();
                        }
                    }
                }
            }
        }


        // 结束单元体
        public void DestroyItem()
        {
            Dispose();
        }
        //靠代理触发
        public void OnCollision(CombatEntity otherCombatEntity)
        {
            if (targetEntity != null)
            {
                if (otherCombatEntity != targetEntity)
                {
                    return;
                }
            }

            var collisionExecuteData = GetComponent<AbilityItemCollisionExecuteComponent>().CollisionExecuteData;

            if (abilityEntity != null)
            {
                if (collisionExecuteData.ActionData.ActionEventType == FireEventType.AssignEffect)
                {
                    if (effectApplyType == EffectApplyType.AllEffects)
                    {
                        abilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(otherCombatEntity, this);
                    }
                    else
                    {
                        abilityEntity.GetComponent<AbilityEffectComponent>().TryAssignEffectToTargetByIndex(otherCombatEntity, (int)effectApplyType - 1);
                    }
                }
            }

            if (abilityExecution != null)
            {
                if (collisionExecuteData.ActionData.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    OnTriggerNewExecution(collisionExecuteData.ActionData);
                }
            }

            if (TryGetComponent(out AbilityItemTargetCounterComponent targetCounterComponent))
            {
                targetCounterComponent.targetCounter++;
            }

            if (targetEntity != null)
            {
                DestroyItem();
            }
        }

        private void OnTriggerNewExecution(ActionEventData actionEventData)
        {
            ExecutionConfigObject executionObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handler, actionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            var sourceExecution = abilityExecution as SkillExecution;
            var execution = sourceExecution.OwnerEntity.AddChildren<SkillExecution, SkillAbility>(sourceExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputPoint = Position;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}
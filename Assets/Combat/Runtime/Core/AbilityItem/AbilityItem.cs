using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 能力单元体
    /// </summary>
    public class AbilityItem : Entity, IPosition
    {
        public Entity AbilityEntity => AbilityExecution.AbilityEntity;
        public IAbilityExecution AbilityExecution { get; set; }
        public EffectApplyType EffectApplyType { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public CombatEntity TargetEntity { get; set; }

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);
       
            AbilityExecution = p1 as IAbilityExecution;     //技能执行体
            if (AbilityEntity == null)
            {
                return;
            }
            var abilityEffects = AbilityEntity.GetComponent<AbilityEffectComponent>().AbilityEffects;
            foreach (var abilityEffect in abilityEffects)
            {
                if (abilityEffect.EffectConfig.Decorators != null)
                {
                    foreach (var effectDecorator in abilityEffect.EffectConfig.Decorators)
                    {
                        if (effectDecorator is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
                        {
                            AddComponent<AbilityItemTargetCounterComponent>();
                        }
                    }
                }
            }
        }


        /// 结束单元体
        public void DestroyItem()
        {
            Dispose();
        }
        //靠代理触发
        public void OnCollision(CombatEntity otherCombatEntity)
        {
            if (TargetEntity != null)
            {
                if (otherCombatEntity != TargetEntity)
                {
                    return;
                }
            }

            var collisionExecuteData = GetComponent<AbilityItemCollisionExecuteComponent>().CollisionExecuteData;

            if (AbilityEntity != null)
            {
                if (collisionExecuteData.ActionData.ActionEventType == FireEventType.AssignEffect)
                {
                    if (EffectApplyType == EffectApplyType.AllEffects)
                    {
                        AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectsToTargetWithAbilityItem(otherCombatEntity, this);
                    }
                    else
                    {
                        AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignEffectByIndex(otherCombatEntity, (int)EffectApplyType - 1);
                    }
                }
            }

            if (AbilityExecution != null)
            {
                if (collisionExecuteData.ActionData.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    OnTriggerNewExecution(collisionExecuteData.ActionData);
                }
            }

            if (TryGetComponent(out AbilityItemTargetCounterComponent targetCounterComponent))
            {
                targetCounterComponent.TargetCounter++;
            }

            if (TargetEntity != null)
            {
                DestroyItem();
            }
        }

        private void OnTriggerNewExecution(ActionEventData ActionEventData)
        {
        
            ExecutionObject executionObject = AssetManager.Instance.LoadAsset<ExecutionObject>(out var handler, ActionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            var sourceExecution = AbilityExecution as SkillExecution;
            var execution = sourceExecution.OwnerEntity.AddChildren<SkillExecution, SkillAbility>(sourceExecution.SkillAbility);
            execution.ExecutionObject = executionObject;
            execution.InputPoint = Position;
            execution.LoadExecutionEffects();
            execution.BeginExecute();
        }
    }
}
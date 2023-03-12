namespace LccModel
{
    public class AbilityItem : Entity
    {
        public IAbilityExecution abilityExecution;
        public Entity abilityEntity;

        public EffectApplyType effectApplyType;

        public TransformComponent TransformComponent => GetComponent<TransformComponent>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            AddComponent<TransformComponent>();

            abilityExecution = p1 as IAbilityExecution;//技能执行体

            abilityEntity = abilityExecution.AbilityEntity;

            if (abilityEntity == null)
            {
                return;
            }
            var abilityEffects = abilityEntity.GetComponent<AbilityEffectComponent>().abilityEffectList;
            foreach (var abilityEffect in abilityEffects)
            {
                if (abilityEffect.effect.DecoratorList != null)
                {
                    foreach (var effectDecorator in abilityEffect.effect.DecoratorList)
                    {
                        if (effectDecorator is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
                        {
                            AddComponent<AbilityItemTargetCounterComponent>();
                        }
                    }
                }
            }
        }


        //靠代理触发
        public void OnCollision(CombatEntity otherCombatEntity)
        {
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

            Dispose();
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
            execution.inputPoint = TransformComponent.position;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}
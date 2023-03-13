namespace LccModel
{
    public class AbilityItem : Entity
    {
        public IAbilityExecution abilityExecution;
        public Entity ability;

        public EffectApplyType effectApplyType;

        public TransformComponent TransformComponent => GetComponent<TransformComponent>();

        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            var clipData = p2 as ExecuteClipData;
            EventManager.Instance.Publish(new SyncCreateAbilityItem(InstanceId)).Coroutine();

            AddComponent<TransformComponent>();

            AddComponent<AbilityItemCollisionExecuteComponent, ExecuteClipData>(clipData);

            abilityExecution = p1 as IAbilityExecution;//技能执行体

            ability = abilityExecution.Ability;

            if (ability == null)
            {
                return;
            }
            var abilityEffects = ability.GetComponent<AbilityEffectComponent>().abilityEffectList;
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
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventManager.Instance.Publish(new SyncDeleteAbilityItem(InstanceId)).Coroutine();
        }

        //靠代理触发
        public void OnCollision(Combat combat)
        {
            var collisionExecuteData = GetComponent<AbilityItemCollisionExecuteComponent>().CollisionExecuteData;

            if (ability != null)
            {
                if (collisionExecuteData.ActionData.ActionEventType == FireEventType.AssignEffect)
                {
                    if (effectApplyType == EffectApplyType.AllEffects)
                    {
                        ability.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(combat, this);
                    }
                    else
                    {
                        ability.GetComponent<AbilityEffectComponent>().TryAssignEffectToTargetByIndex(combat, (int)effectApplyType - 1);
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

            var targetCounterComponent = GetComponent<AbilityItemTargetCounterComponent>();
            if (targetCounterComponent != null)
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
            var execution = sourceExecution.Owner.AddChildren<SkillExecution, SkillAbility>(sourceExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputPoint = TransformComponent.position;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}
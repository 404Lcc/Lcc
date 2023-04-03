using UnityEngine;

namespace LccModel
{
    public class AbilityItem : Entity, IUpdate
    {
        public IAbilityExecution abilityExecution;
        public Entity ability;

        public EffectApplyType effectApplyType;

        public TransformComponent TransformComponent => GetComponent<TransformComponent>();
        public AABB2DComponent AABB2DComponent => GetComponent<AABB2DComponent>();

        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            var clipData = p2 as ExecuteClipData;
            EventSystem.Instance.Publish(new SyncCreateAbilityItem(InstanceId));

            AddComponent<TransformComponent>();

            AddComponent<AbilityItemCollisionExecuteComponent, ExecuteClipData>(clipData);

            AddComponent<AABB2DComponent, Vector2, Vector2>(new Vector2(-1, -1), new Vector2(1, 1));

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

        public void Update()
        {
            foreach (var item in CombatContext.Instance.combatDict.Values)
            {
                if (AABB2DComponent.Intersects(item.AABB2DComponent))
                {
                    OnCollision(item);
                }
            }
        }


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

            EventSystem.Instance.Publish(new SyncDeleteAbilityItem(InstanceId));

            CombatContext.Instance.RemoveAbilityItem(InstanceId);
        }

        private void OnTriggerNewExecution(ActionEventData actionEventData)
        {
            var sourceExecution = abilityExecution as SkillExecution;
            ExecutionConfigObject executionObject = sourceExecution.Owner.AttachExecution(actionEventData.NewExecutionId);
            if (executionObject == null)
            {
                return;
            }


            var execution = sourceExecution.Owner.AddChildren<SkillExecution, SkillAbility>(sourceExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputPoint = TransformComponent.position;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}
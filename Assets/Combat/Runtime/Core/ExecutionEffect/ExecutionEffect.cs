using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ExecuteEffectEvent
    {
        public ExecutionEffect ExecutionEffect;
    }

    /// <summary>
    /// 执行体效果，针对于技能执行体的效果，如播放动作、生成碰撞体、位移等这些和技能表现相关的效果
    /// </summary>
    public partial class ExecutionEffect : Entity
    {
        public ExecuteClipData ExecutionEffectConfig { get; set; }
        public SkillExecution ParentExecution => GetParent<SkillExecution>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);


            ExecutionEffectConfig = p1 as ExecuteClipData;
            //Name = ExecutionEffectConfig.GetType().Name;

            var clipType = ExecutionEffectConfig.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                var spawnItemEffect = ExecutionEffectConfig.ActionEventData;
                //应用效果给目标效果
                if (spawnItemEffect.ActionEventType == FireEventType.AssignEffect)
                {
                    AddComponent<ExecutionAssignToTargetComponent>().EffectApplyType = spawnItemEffect.EffectApply;
                }
                //触发新的执行体效果
                if (spawnItemEffect.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    AddComponent<ExecutionTriggerNewExecutionComponent>().ActionEventData = spawnItemEffect;
                }
            }


            //生成碰撞体效果，碰撞体再触发应用能力效果
            if (clipType == ExecuteClipType.CollisionExecute)
            {
                var spawnItemEffect = ExecutionEffectConfig.CollisionExecuteData;
                AddComponent<ExecutionSpawnCollisionComponent>().CollisionExecuteData = spawnItemEffect;
            }
            //播放动作效果
            if (clipType == ExecuteClipType.Animation)
            {
                var animationEffect = ExecutionEffectConfig.AnimationData;
                AddComponent<ExecutionAnimationComponent>().AnimationClip = animationEffect.AnimationClip;
            }
            //播放特效效果
            if (clipType == ExecuteClipType.ParticleEffect)
            {
                var animationEffect = ExecutionEffectConfig.ParticleEffectData;
                AddComponent<ExecutionParticleEffectComponent>().ParticleEffectPrefab = animationEffect.ParticleEffect;
            }

            //时间到触发执行效果
            if (clipType == ExecuteClipType.ActionEvent)
            {
                AddComponent<ExecutionTimeTriggerComponent>().StartTime = ExecutionEffectConfig.StartTime;
            }
            else if (ExecutionEffectConfig.Duration > 0)
            {
                AddComponent<ExecutionTimeTriggerComponent>().StartTime = ExecutionEffectConfig.StartTime;
                GetComponent<ExecutionTimeTriggerComponent>().EndTime = ExecutionEffectConfig.EndTime;
            }
        }

        public void BeginExecute()
        {
            if (!TryGetComponent(out ExecutionTimeTriggerComponent timeTriggerComponent))
            {
                TriggerEffect();
            }
            foreach (var item in Components.Values)
            {
                ((Component)item).Enable = true;
            }
        }

        public void TriggerEffect()
        {
            this.Publish(new ExecuteEffectEvent()
            {
                ExecutionEffect = this
            });
            this.FireEvent(nameof(TriggerEffect));
        }

        public void EndEffect()
        {
            this.FireEvent(nameof(EndEffect));
        }
    }
}

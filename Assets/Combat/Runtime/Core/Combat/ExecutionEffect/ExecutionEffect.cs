namespace LccModel
{
    public partial class ExecutionEffect : Entity
    {
        public ExecuteClipData executeClipData;
        public SkillExecution Execution => GetParent<SkillExecution>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);


            executeClipData = p1 as ExecuteClipData;

            var clipType = executeClipData.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                var spawnItemEffect = executeClipData.ActionEventData;
                //应用效果给目标效果
                if (spawnItemEffect.ActionEventType == FireEventType.AssignEffect)
                {
                    AddComponent<ExecutionEffectAssignToTargetComponent>().effectApplyType = spawnItemEffect.EffectApply;
                }
                //触发新的执行体效果
                if (spawnItemEffect.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    AddComponent<ExecutionEffectTriggerNewExecutionComponent>().actionEventData = spawnItemEffect;
                }
            }


            //生成碰撞体效果，碰撞体再触发应用能力效果
            if (clipType == ExecuteClipType.CollisionExecute)
            {
                var spawnItemEffect = executeClipData.CollisionExecuteData;
                AddComponent<ExecutionEffectSpawnCollisionComponent>().collisionExecuteData = spawnItemEffect;
            }
            //播放动作效果
            if (clipType == ExecuteClipType.Animation)
            {
                var animationEffect = executeClipData.AnimationData;
                AddComponent<ExecutionEffectAnimationComponent>().animationClip = animationEffect.AnimationClip;
            }
            //播放特效效果
            if (clipType == ExecuteClipType.ParticleEffect)
            {
                var animationEffect = executeClipData.ParticleEffectData;
                AddComponent<ExecutionEffectParticleEffectComponent>().particleEffectPrefab = animationEffect.ParticleEffect;
            }

            //时间到触发执行效果
            if (clipType == ExecuteClipType.ActionEvent)
            {
                AddComponent<ExecutionEffectTimeTriggerComponent>().startTime = executeClipData.StartTime;
            }
            else if (executeClipData.Duration > 0)
            {
                AddComponent<ExecutionEffectTimeTriggerComponent>().startTime = executeClipData.StartTime;
                GetComponent<ExecutionEffectTimeTriggerComponent>().endTime = executeClipData.EndTime;
            }
        }

        public void BeginExecute()
        {
            if (!TryGetComponent(out ExecutionEffectTimeTriggerComponent timeTriggerComponent))
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
            this.Publish(this);
            this.FireEvent(nameof(TriggerEffect));
        }

        public void EndEffect()
        {
            this.FireEvent(nameof(EndEffect));
        }
    }
}
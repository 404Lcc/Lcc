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

            ExecuteClipType clipType = executeClipData.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                if (executeClipData.ActionEventData.ActionEventType == FireEventType.AssignEffect)
                {
                    AddComponent<ExecutionEffectAssignToTargetComponent>().effectApplyType = executeClipData.ActionEventData.EffectApply;
                }

                if (executeClipData.ActionEventData.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    AddComponent<ExecutionEffectTriggerNewExecutionComponent>().actionEventData = executeClipData.ActionEventData;
                }
            }


            if (clipType == ExecuteClipType.CollisionExecute)
            {
                AddComponent<ExecutionEffectSpawnCollisionComponent>().collisionExecuteData = executeClipData.CollisionExecuteData;
            }

            if (clipType == ExecuteClipType.Animation)
            {
                AddComponent<ExecutionEffectAnimationComponent>().animationClip = executeClipData.AnimationData.AnimationClip;
            }

            if (clipType == ExecuteClipType.ParticleEffect)
            {
                AddComponent<ExecutionEffectParticleEffectComponent>().particleEffectPrefab = executeClipData.ParticleEffectData.ParticleEffect;
            }


        }

        public void BeginExecute()
        {
            ExecuteClipType clipType = executeClipData.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                AddComponent<ExecutionEffectTimeTriggerComponent>().startTime = executeClipData.StartTime;
            }
            else if (executeClipData.Duration > 0)
            {
                AddComponent<ExecutionEffectTimeTriggerComponent>().startTime = executeClipData.StartTime;
                GetComponent<ExecutionEffectTimeTriggerComponent>().endTime = executeClipData.EndTime;
            }

            if (GetComponent<ExecutionEffectTimeTriggerComponent>() == null)
            {
                TriggerEffect();
            }
        }

        public void TriggerEffect()
        {
            Publish(this);
            FireEvent(nameof(TriggerEffect));
        }

        public void EndEffect()
        {
            FireEvent(nameof(EndEffect));
        }
    }
}
using System;

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
                if (executeClipData.ActionEventData.ActionEventType == FireEventType.FiltrationTarget)
                {
                    AddComponent<ExecutionEffectFiltrationTargetComponent>();
                }

                if (executeClipData.ActionEventData.ActionEventType == FireEventType.AssignEffect)
                {
                    AddComponent<ExecutionEffectAssignToTargetComponent>();
                }

                if (executeClipData.ActionEventData.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    AddComponent<ExecutionEffectTriggerNewExecutionComponent>();
                }
            }


            if (clipType == ExecuteClipType.CollisionExecute)
            {
                AddComponent<ExecutionEffectSpawnCollisionComponent>();
            }

            if (clipType == ExecuteClipType.Animation)
            {
                AddComponent<ExecutionEffectAnimationComponent>();
            }

            if (clipType == ExecuteClipType.ParticleEffect)
            {
                AddComponent<ExecutionEffectParticleEffectComponent>();
            }


        }

        public void BeginExecute()
        {
            ExecuteClipType clipType = executeClipData.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                AddComponent<ExecutionEffectTimeTriggerComponent>().startTime = (long)(executeClipData.StartTime * 1000);
            }
            else if (executeClipData.Duration > 0)
            {
                AddComponent<ExecutionEffectTimeTriggerComponent>().startTime = (long)(executeClipData.StartTime * 1000);
                GetComponent<ExecutionEffectTimeTriggerComponent>().endTime = (long)(executeClipData.EndTime * 1000);
            }

            if (GetComponent<ExecutionEffectTimeTriggerComponent>() == null)
            {
                StartTriggerEffect();
            }
        }

        public void StartTriggerEffect()
        {
            ExecuteClipType clipType = executeClipData.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                if (executeClipData.ActionEventData.ActionEventType == FireEventType.FiltrationTarget)
                {
                    GetComponent<ExecutionEffectFiltrationTargetComponent>().OnTriggerExecutionEffect(this);
                }

                if (executeClipData.ActionEventData.ActionEventType == FireEventType.AssignEffect)
                {
                    GetComponent<ExecutionEffectAssignToTargetComponent>().OnTriggerExecutionEffect(this);
                }

                if (executeClipData.ActionEventData.ActionEventType == FireEventType.TriggerNewExecution)
                {
                    GetComponent<ExecutionEffectTriggerNewExecutionComponent>().OnTriggerExecutionEffect(this);
                }
            }


            if (clipType == ExecuteClipType.CollisionExecute)
            {
                GetComponent<ExecutionEffectSpawnCollisionComponent>().OnTriggerExecutionEffect(this);
            }

            if (clipType == ExecuteClipType.Animation)
            {
                GetComponent<ExecutionEffectAnimationComponent>().OnTriggerExecutionEffect(this);
            }

            if (clipType == ExecuteClipType.ParticleEffect)
            {
                GetComponent<ExecutionEffectParticleEffectComponent>().OnTriggerExecutionEffect(this);
            }
        }

        public void EndEffect()
        {
            ExecuteClipType clipType = executeClipData.ExecuteClipType;
            if (clipType == ExecuteClipType.ActionEvent)
            {
                if (executeClipData.ActionEventData.ActionEventType == FireEventType.FiltrationTarget)
                {
                }
                if (executeClipData.ActionEventData.ActionEventType == FireEventType.AssignEffect)
                {

                }

                if (executeClipData.ActionEventData.ActionEventType == FireEventType.TriggerNewExecution)
                {

                }
            }


            if (clipType == ExecuteClipType.CollisionExecute)
            {
        
            }

            if (clipType == ExecuteClipType.Animation)
            {
         
            }

            if (clipType == ExecuteClipType.ParticleEffect)
            {
                GetComponent<ExecutionEffectParticleEffectComponent>().OnTriggerExecutionEffectEnd(this);
            }
        }
    }
}
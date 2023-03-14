namespace LccModel
{
    public class AbilityItemCollisionExecuteComponent : Component
    {
        public ExecuteClipData executeClipData;
        public CollisionExecuteData CollisionExecuteData => executeClipData.CollisionExecuteData;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            executeClipData = p1 as ExecuteClipData;
            if (CollisionExecuteData.ActionData.ActionEventType == FireEventType.AssignEffect)
            {
                GetParent<AbilityItem>().effectApplyType = CollisionExecuteData.ActionData.EffectApply;
            }
            if (CollisionExecuteData.ActionData.ActionEventType == FireEventType.TriggerNewExecution)
            {

            }
        }
    }
}
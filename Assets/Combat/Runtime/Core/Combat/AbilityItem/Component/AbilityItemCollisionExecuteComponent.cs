namespace LccModel
{
    public class AbilityItemCollisionExecuteComponent : Component
    {
        public ExecuteClipData ExecuteClipData { get; private set; }
        public CollisionExecuteData CollisionExecuteData => ExecuteClipData.CollisionExecuteData;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            ExecuteClipData = p1 as ExecuteClipData;
            if (CollisionExecuteData.ActionData.ActionEventType == FireEventType.AssignEffect)
            {
                GetParent<AbilityItem>().EffectApplyType = CollisionExecuteData.ActionData.EffectApply;
            }
            if (CollisionExecuteData.ActionData.ActionEventType == FireEventType.TriggerNewExecution)
            {

            }
        }
    }
}
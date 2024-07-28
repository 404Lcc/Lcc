namespace LccModel
{
    public class Idle2WalkState : AStateTransition
    {
        public override bool TransitionCondition()
        {
            return true;
        }
    }
}
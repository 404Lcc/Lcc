namespace LccModel
{
    public abstract class AStateTransition
    {
        public Combat combat;
        public FSMComponet fsmComponent;

        public FSMStateType currentStateType;
        public FSMStateType nextStateType;
        public abstract bool TransitionCondition();
    }
}
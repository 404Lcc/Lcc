namespace LccModel
{
    public abstract class AFSMState
    {
        public Combat combat;
        public AIComponent aiComponent;
        public abstract FSMStateType State
        {
            get;
        }
        public abstract void EnterState();
        public abstract void FixedUpdate();
        public abstract void LevelState();
    }
}
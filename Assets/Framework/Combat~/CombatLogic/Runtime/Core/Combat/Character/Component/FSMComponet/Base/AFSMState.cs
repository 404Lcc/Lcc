using System.Collections.Generic;

namespace LccModel
{
    public abstract class AFSMState
    {
        private List<AStateTransition> stateTransitionList = new List<AStateTransition>();
        public Combat combat;
        public FSMComponet fsmComponent;
        public abstract FSMStateType State
        {
            get;
        }
        public abstract void EnterState();
        public virtual void FixedUpdate()
        {
            foreach (var item in stateTransitionList)
            {
                if (item.TransitionCondition())
                {
                    fsmComponent.EnterState(fsmComponent.GetState(item.nextStateType));
                }
            }
        }
        public abstract void LevelState();

        public virtual void AddStateTransition<T>(FSMStateType currentState, FSMStateType nextState, T state) where T : AStateTransition
        {
            state.combat = combat;
            state.fsmComponent = fsmComponent;
            state.currentStateType = currentState;
            state.nextStateType = nextState;

            stateTransitionList.Add(state);
        }
    }
}
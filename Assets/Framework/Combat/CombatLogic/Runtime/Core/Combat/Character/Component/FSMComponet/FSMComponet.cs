using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class FSMComponet : Component, IFixedUpdate
    {
        public Dictionary<FSMStateType, AFSMState> stateDict = new Dictionary<FSMStateType, AFSMState>();
        public AFSMState Current { get; private set; }

        public void AddState(AFSMState state)
        {
            if (stateDict.ContainsKey(state.State)) return;
            stateDict.Add(state.State, state);
        }
        public AFSMState GetState(FSMStateType type)
        {
            if (stateDict.TryGetValue(type, out var state))
            {
                return state;
            }
            return null;
        }

        public void EnterState(AFSMState newState)
        {
            if (Current != null)
            {
                Current.LevelState();
            }
            Current = null;
            newState.combat = GetParent<Combat>();
            newState.fsmComponent = this;
            newState.EnterState();
            Current = newState;
        }

        public void FixedUpdate()
        {
            if (Current == null) return;
            Current.FixedUpdate();
        }
    }
}
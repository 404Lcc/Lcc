using UnityEngine;

namespace LccModel
{
    public class IdleState : AFSMState
    {
        public override FSMStateType State => FSMStateType.Idle;

        public override void EnterState()
        {
        }

        public override void LevelState()
        {
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}
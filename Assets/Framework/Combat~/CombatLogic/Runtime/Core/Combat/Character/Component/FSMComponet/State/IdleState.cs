using UnityEngine;

namespace LccModel
{
    public class IdleState : AFSMState
    {
        public override FSMStateType State => FSMStateType.Idle;

        public override void EnterState()
        {
            combat.AnimationComponent.PlayAnimation(AnimationType.Idle);
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
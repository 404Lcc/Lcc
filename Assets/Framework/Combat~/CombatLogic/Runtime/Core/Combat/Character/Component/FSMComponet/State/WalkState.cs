using UnityEngine;

namespace LccModel
{
    public class WalkState : AFSMState
    {
        public override FSMStateType State => FSMStateType.Walk;

        public override void EnterState()
        {
            combat.AnimationComponent.PlayAnimation(AnimationType.Walk);
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
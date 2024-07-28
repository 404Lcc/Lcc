using UnityEngine;

namespace LccModel
{
    public class AttackState : AFSMState
    {
        public override FSMStateType State => FSMStateType.Attack;

        public override void EnterState()
        {
            combat.AnimationComponent.PlayAnimation(AnimationType.Attack);
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

using UnityEngine;

namespace LccModel
{
    public class JoystickComponent : Component, IFixedUpdate
    {
        public Combat CombatEntity => GetParent<Combat>();
        public TransformComponent TransformComponent => CombatEntity.TransformComponent;
        public AnimationComponent AnimationComponent => CombatEntity.AnimationComponent;
        public float speed => CombatEntity.AttributeComponent.MoveSpeed.Value;
        public Vector2 normalDistance;
        public void FixedUpdate()
        {
            if (normalDistance != Vector2.zero)
            {
                TransformComponent.Translate(normalDistance * UnityEngine.Time.deltaTime * speed);
            }
        }
        public void Move(Vector2 normalDistance, float angle)
        {
            this.normalDistance = normalDistance;
            if (normalDistance != Vector2.zero)
            {
                AnimationComponent.PlayAnimation(AnimationType.Run);
            }
            else
            {
                AnimationComponent.PlayAnimation(AnimationType.Idle);
            }
            if (angle == 0) return;
            if (angle < 0)
            {
                TransformComponent.rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                TransformComponent.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
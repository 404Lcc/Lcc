namespace LccModel
{
    public class MotionComponent : Component
    {
        public TransformComponent TransformComponent => GetParent<CombatEntity>().GetComponent<TransformComponent>();

        public void SetEnable(bool enable)
        {

        }

    }
}
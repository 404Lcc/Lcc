namespace LccModel
{
    public class MotionComponent : Component
    {
        public TransformComponent TransformComponent => GetParent<Combat>().GetComponent<TransformComponent>();

        public void SetEnable(bool enable)
        {

        }

    }
}
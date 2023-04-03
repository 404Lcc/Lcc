namespace LccModel
{
    public enum AnimationType
    {
        Idle,
        Walk,
        Attack,
        Dead,
    }
    public class AnimationComponent : Component
    {
        public AnimationType currentType;

        public void PlayAnimation(AnimationType type, float speed = 1)
        {
            currentType = type;
            bool isLoop = currentType == AnimationType.Idle || currentType == AnimationType.Walk ? true : false;
            EventSystem.Instance.Publish(new SyncAnimation(GetParent<Combat>().InstanceId, type, speed, isLoop));
        }
    }
}
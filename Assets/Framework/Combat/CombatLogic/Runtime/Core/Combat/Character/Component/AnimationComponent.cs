namespace LccModel
{
    public enum AnimationType
    {
        Attack1,
        Attack2,
        Idle,
        Walk,
        Run,
        FullJump,
        Jump1,
        Jump2,
        Jump3,
        Buff,
        Hurt,
        Special,
        Death,
    }
    public class AnimationComponent : Component
    {
        public AnimationType currentType;

        public void PlayAnimation(AnimationType type, float speed = 1)
        {
            if (currentType == type) return;
            currentType = type;
            bool isLoop = currentType == AnimationType.Run || currentType == AnimationType.Idle || currentType == AnimationType.Walk ? true : false;
            EventManager.Instance.Publish(new SyncAnimation(GetParent<Combat>().InstanceId, type, speed, isLoop));
        }
    }
}
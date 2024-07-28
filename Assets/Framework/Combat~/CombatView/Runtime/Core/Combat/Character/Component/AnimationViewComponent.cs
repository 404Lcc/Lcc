using System.Collections.Generic;

namespace LccModel
{
    public class AnimationViewComponent : Component
    {
        public AnimationType currentType;
        public bool isPlaying = false;

        private Dictionary<AnimationType, string> _enemyAnimationDict = new Dictionary<AnimationType, string>();//π÷ŒÔ


        public override void Start()
        {
            base.Start();

            _enemyAnimationDict.Add(AnimationType.Idle, "Idle");
            _enemyAnimationDict.Add(AnimationType.Walk, "Walk");
            _enemyAnimationDict.Add(AnimationType.Attack, "Attack");
            _enemyAnimationDict.Add(AnimationType.Dead, "Dead");

        }

        public void PlayAnimation(AnimationType type, float speed, bool isLoop)
        {
        }
    }
}
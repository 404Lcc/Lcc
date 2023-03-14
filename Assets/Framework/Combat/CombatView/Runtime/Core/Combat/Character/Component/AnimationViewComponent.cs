using System.Collections.Generic;

namespace LccModel
{
    public class AnimationViewComponent : Component
    {
        public AnimationType current;



        private Dictionary<AnimationType, string> _warriorAnimationDict = new Dictionary<AnimationType, string>();//战士
        private Dictionary<AnimationType, string> _archerAnimationDict = new Dictionary<AnimationType, string>();//弓箭手
        private Dictionary<AnimationType, string> _elementalistAnimationDict = new Dictionary<AnimationType, string>();//法师
        private Dictionary<AnimationType, string> _duelistAnimationDict = new Dictionary<AnimationType, string>();//双手
        private Dictionary<AnimationType, string> _enemyAnimationDict = new Dictionary<AnimationType, string>();//怪物


        public override void Start()
        {
            base.Start();

            _warriorAnimationDict.Add(AnimationType.Attack1, "Attack1");
            _warriorAnimationDict.Add(AnimationType.Attack2, "Attack2");
            _warriorAnimationDict.Add(AnimationType.Idle, "Idle");
            _warriorAnimationDict.Add(AnimationType.Walk, "Walk");
            _warriorAnimationDict.Add(AnimationType.Run, "Run");
            _warriorAnimationDict.Add(AnimationType.FullJump, "Jump");
            _warriorAnimationDict.Add(AnimationType.Jump1, "Jump1");
            _warriorAnimationDict.Add(AnimationType.Jump2, "Jump2");
            _warriorAnimationDict.Add(AnimationType.Jump3, "Jump3");
            _warriorAnimationDict.Add(AnimationType.Buff, "Buff");
            _warriorAnimationDict.Add(AnimationType.Hurt, "Hurt");
            _warriorAnimationDict.Add(AnimationType.Special, "Defence");
            _warriorAnimationDict.Add(AnimationType.Death, "Death");

            _archerAnimationDict.Add(AnimationType.Attack1, "Shoot1");
            _archerAnimationDict.Add(AnimationType.Attack2, "Shoot2");
            _archerAnimationDict.Add(AnimationType.Idle, "Idle ARCHER");
            _archerAnimationDict.Add(AnimationType.Walk, "Walk");
            _archerAnimationDict.Add(AnimationType.Run, "Run ARCHER");
            _archerAnimationDict.Add(AnimationType.FullJump, "Jump");
            _archerAnimationDict.Add(AnimationType.Jump1, "Jump1 ARCHER");
            _archerAnimationDict.Add(AnimationType.Jump2, "Jump2");
            _archerAnimationDict.Add(AnimationType.Jump3, "Jump3 ARCHER");
            _archerAnimationDict.Add(AnimationType.Buff, "Buff");
            _archerAnimationDict.Add(AnimationType.Hurt, "Hurt");
            _archerAnimationDict.Add(AnimationType.Special, "Shoot3");
            _archerAnimationDict.Add(AnimationType.Death, "Death");

            _elementalistAnimationDict.Add(AnimationType.Attack1, "Cast1");
            _elementalistAnimationDict.Add(AnimationType.Attack2, "Cast2");
            _elementalistAnimationDict.Add(AnimationType.Idle, "Idle");
            _elementalistAnimationDict.Add(AnimationType.Walk, "Walk");
            _elementalistAnimationDict.Add(AnimationType.Run, "Fly");
            _elementalistAnimationDict.Add(AnimationType.FullJump, "Jump");
            _elementalistAnimationDict.Add(AnimationType.Jump1, "Jump1");
            _elementalistAnimationDict.Add(AnimationType.Jump2, "Jump2");
            _elementalistAnimationDict.Add(AnimationType.Jump3, "Jump3");
            _elementalistAnimationDict.Add(AnimationType.Buff, "Buff");
            _elementalistAnimationDict.Add(AnimationType.Hurt, "Hurt");
            _elementalistAnimationDict.Add(AnimationType.Special, "Cast3");
            _elementalistAnimationDict.Add(AnimationType.Death, "Death");

            _duelistAnimationDict.Add(AnimationType.Attack1, "Attack 1 DUELIST");
            _duelistAnimationDict.Add(AnimationType.Attack2, "Attack 2 DUELIST");
            _duelistAnimationDict.Add(AnimationType.Idle, "Idle");
            _duelistAnimationDict.Add(AnimationType.Walk, "Walk");
            _duelistAnimationDict.Add(AnimationType.Run, "Run DUELIST");
            _duelistAnimationDict.Add(AnimationType.FullJump, "Jump");
            _duelistAnimationDict.Add(AnimationType.Jump1, "Jump1");
            _duelistAnimationDict.Add(AnimationType.Jump2, "Jump2");
            _duelistAnimationDict.Add(AnimationType.Jump3, "Jump3");
            _duelistAnimationDict.Add(AnimationType.Buff, "Buff");
            _duelistAnimationDict.Add(AnimationType.Hurt, "Hurt");
            _duelistAnimationDict.Add(AnimationType.Special, "Attack 3 DUELIST");
            _duelistAnimationDict.Add(AnimationType.Death, "Death");

            _enemyAnimationDict.Add(AnimationType.Idle, "Idle");
            _enemyAnimationDict.Add(AnimationType.Walk, "Walk");
            _enemyAnimationDict.Add(AnimationType.Death, "Dead");
            _enemyAnimationDict.Add(AnimationType.Attack1, "Attack");

        }

        public void PlayAnimation(AnimationType type, float speed, bool isLoop)
        {

        }
    }
}
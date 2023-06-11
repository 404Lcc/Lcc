using UnityEngine;

namespace LccModel
{
    public class IdleState : AFSMState
    {
        public Vector3 initialPosition => aiComponent.initialPosition;

        public float wanderRadius => aiComponent.wanderRadius;          //游走半径，移动状态下，如果超出游走半径会返回出生位置
        public float defendRadius => aiComponent.defendRadius;          //自卫半径，玩家进入后怪物会追击玩家，当距离<攻击距离则会发动攻击（或者触发战斗）
        public float chaseRadius => aiComponent.chaseRadius;           //追击半径，当怪物超出追击半径后会放弃追击，返回追击起始位置
        public float attackRange => aiComponent.attackRange;           //攻击距离


        public float[] actionWeight => aiComponent.actionWeight;        //设置待机时各种动作的权重，顺序依次为呼吸、观察、移动
        public float actRestTme => aiComponent.actRestTme;           //更换待机指令的间隔时间
        public float attackTime => aiComponent.attackTime;        //攻击的间隔时间

        public Combat target
        {
            get
            {
                if (aiComponent.target != null && !aiComponent.target.IsDisposed)
                {
                    return aiComponent.target;
                }
                return null;
            }
            set
            {
                aiComponent.target = value;
            }
        }




        private float diatanceToPlayer;                                         //怪物与玩家的距离
        private float diatanceToInitial;                                     //怪物与初始位置的距离


        public GameTimer randomStateTimer;
        public override FSMStateType State => FSMStateType.Idle;

        public override void EnterState()
        {
            randomStateTimer = new GameTimer(actRestTme);

            combat.AnimationComponent.PlayAnimation(AnimationType.Idle);
        }

        public override void LevelState()
        {
            randomStateTimer = null;
        }

        public override void FixedUpdate()
        {
            if (randomStateTimer != null && !randomStateTimer.IsFinished)
            {
                randomStateTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, RandomState);
            }
            Check();
        }
        private void RandomState()
        {
            float number = Random.Range(0, actionWeight[0] + actionWeight[1]);
            if (number <= actionWeight[0])
            {
                aiComponent.SetState(new IdleState());
            }
            else if (actionWeight[0] < number && number <= actionWeight[0] + actionWeight[1])
            {
                aiComponent.SetState(new WalkState());
            }
        }
        private void Check()
        {
            target = FiltrationTarget.GetTarget(combat.TransformComponent, chaseRadius, TagType.Player);
            if (target != null)
            {
                if (FiltrationTarget.IsIncludeTarget(combat.TransformComponent, target.TransformComponent))
                {
                    diatanceToPlayer = Vector3.Distance(target.TransformComponent.position, combat.TransformComponent.position);
                    if (diatanceToPlayer < attackRange)
                    {
                        aiComponent.SetState(new AttackState());
                    }
                    else if (diatanceToPlayer < chaseRadius)
                    {
                        aiComponent.SetState(new ChaseState());
                    }
                }
                else
                {
                    aiComponent.SetState(new ChaseState());
                }



            }


            diatanceToInitial = Vector3.Distance(combat.TransformComponent.position, aiComponent.initialPosition);

            if (diatanceToInitial > wanderRadius)
            {
                aiComponent.SetState(new ReturnState());
            }
        }
    }
}
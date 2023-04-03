using UnityEngine;

namespace LccModel
{
    public class AttackState : AFSMState
    {
        public Vector3 initialPosition => aiComponent.initialPosition;

        public float wanderRadius => aiComponent.wanderRadius;          //���߰뾶���ƶ�״̬�£�����������߰뾶�᷵�س���λ��
        public float defendRadius => aiComponent.defendRadius;          //�����뾶����ҽ��������׷����ң�������<����������ᷢ�����������ߴ���ս����
        public float chaseRadius => aiComponent.chaseRadius;           //׷���뾶�������ﳬ��׷���뾶������׷��������׷����ʼλ��
        public float attackRange => aiComponent.attackRange;           //��������
   

        public float[] actionWeight => aiComponent.actionWeight;        //���ô���ʱ���ֶ�����Ȩ�أ�˳������Ϊ�������۲졢�ƶ�
        public float actRestTme => aiComponent.actRestTme;           //��������ָ��ļ��ʱ��
        public float attackTime => aiComponent.attackTime;        //�����ļ��ʱ��

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






        private float diatanceToPlayer;                                         //��������ҵľ���
        private float diatanceToInitial;                                     //�������ʼλ�õľ���






        public GameTimer randomStateTimer;

        public GameTimer attackTimer;
        public override FSMStateType State => FSMStateType.Attack;

        public override void EnterState()
        {
            randomStateTimer = new GameTimer(actRestTme);
            attackTimer = new GameTimer(attackTime);

            combat.AnimationComponent.PlayAnimation(AnimationType.Idle);
        }

        public override void LevelState()
        {
            randomStateTimer = null;
            attackTimer = null;
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
        public override void FixedUpdate()
        {



            Check();
        }

        private void Check()
        {
            if (target == null)
            {
                aiComponent.SetState(new ChaseState());
                return;
            }
            if (FiltrationTarget.IsIncludeTarget(combat.TransformComponent, target.TransformComponent))
            {
                diatanceToPlayer = Vector3.Distance(target.TransformComponent.position, combat.TransformComponent.position);
                if (diatanceToPlayer < attackRange)
                {
                    randomStateTimer.Reset();
                    if (attackTimer != null && !attackTimer.IsFinished)
                    {
                        attackTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, () =>
                        {
                            combat.GetComponent<SpellSkillComponent>().Spell(combat.GetSkill(1));
                        });
                    }
                }
                else if (diatanceToPlayer < defendRadius)
                {
                    aiComponent.SetState(new ChaseState());
                }
                else
                {
                    attackTimer.Reset();
                    if (randomStateTimer != null && !randomStateTimer.IsFinished)
                    {
                        randomStateTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, RandomState);
                    }
                }
            }
            else
            {
                aiComponent.SetState(new ChaseState());
            }

            diatanceToInitial = Vector3.Distance(combat.TransformComponent.position, initialPosition);

            if (diatanceToInitial > wanderRadius)
            {
                aiComponent.SetState(new ReturnState());
            }
        }
    }
}
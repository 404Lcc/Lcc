using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ReturnState : AFSMState
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



        public override FSMStateType State => FSMStateType.Return;

        public override void EnterState()
        {
            combat.AnimationComponent.PlayAnimation(AnimationType.Walk);
        }

        public override void LevelState()
        {
            

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
            combat.TransformComponent.MoveToTarget(initialPosition);

            Check();
        }
        private void Check()
        {
            diatanceToInitial = Vector3.Distance(combat.TransformComponent.position, initialPosition);

            if (diatanceToInitial < 0.5f)
            {
                RandomState();
            }
        }
    }
}
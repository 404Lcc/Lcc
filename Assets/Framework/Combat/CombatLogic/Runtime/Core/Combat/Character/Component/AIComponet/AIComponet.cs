using UnityEngine;

namespace LccModel
{
    public class AIComponent : Component, IFixedUpdate
    {
        public Vector3 initialPosition;

        public float wanderRadius = 20;          //���߰뾶���ƶ�״̬�£�����������߰뾶�᷵�س���λ��
        public float defendRadius = 10;          //�����뾶����ҽ��������׷����ң�������<����������ᷢ�����������ߴ���ս����
        public float chaseRadius = 15;            //׷���뾶�������ﳬ��׷���뾶������׷��������׷����ʼλ��
        public float attackRange = 2;            //��������


        public float[] actionWeight = { 3000, 4000 };         //���ô���ʱ���ֶ�����Ȩ�أ�˳������Ϊ�������۲졢�ƶ�
        public long actRestTme = 3000;            //��������ָ��ļ��ʱ��
        public long attackTime = 2000;            //�����ļ��ʱ��


        public Combat target;


        public AFSMState Current { get; private set; }
        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            initialPosition = (Vector3)datas[0];
        }
        public void SetState(AFSMState newState)
        {
            if (Current != null)
            {
                Current.LevelState();
            }
            Current = null;
            newState.combat = GetParent<Combat>();
            newState.aiComponent = this;
            newState.EnterState();
            Current = newState;
        }
        public void FixedUpdate()
        {
            if (Current == null) return;
            Current.FixedUpdate();
        }
    }
}
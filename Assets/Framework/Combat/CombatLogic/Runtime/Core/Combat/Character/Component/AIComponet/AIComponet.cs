using UnityEngine;

namespace LccModel
{
    public class AIComponent : Component, IFixedUpdate
    {
        public Vector3 initialPosition;

        public float wanderRadius = 20;          //游走半径，移动状态下，如果超出游走半径会返回出生位置
        public float defendRadius = 10;          //自卫半径，玩家进入后怪物会追击玩家，当距离<攻击距离则会发动攻击（或者触发战斗）
        public float chaseRadius = 15;            //追击半径，当怪物超出追击半径后会放弃追击，返回追击起始位置
        public float attackRange = 2;            //攻击距离


        public float[] actionWeight = { 3000, 4000 };         //设置待机时各种动作的权重，顺序依次为呼吸、观察、移动
        public long actRestTme = 3000;            //更换待机指令的间隔时间
        public long attackTime = 2000;            //攻击的间隔时间


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
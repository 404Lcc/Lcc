using System;
using System.Collections.Generic;
using LccModel;

namespace LccHotfix
{
    public class StateNode : IStateNode
    {
        protected FSM fsm;
        protected LogicEntity entity;

        public void ChangeState<T>() where T : IStateNode
        {
            fsm.ChangeState<T>();
        }

        public virtual void OnCreate(StateMachine machine)
        {
            fsm = machine as FSM;
            entity = fsm.Entity;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HybridCLR;

namespace LccModel
{
    public class GameAction
    {
        public event Action OnFixedUpdate;
        public event Action OnUpdate;
        public event Action OnLateUpdate;
        public event Action OnClose;
        public event Action OnDrawGizmos;

        public void ExecuteOnFixedUpdate()
        {
            if (OnFixedUpdate != null)
            {
                OnFixedUpdate();
            }
        }

        public void ExecuteOnUpdate()
        {
            if (OnUpdate != null)
            {
                OnUpdate();
            }
        }

        public void ExecuteOnLateUpdate()
        {
            if (OnLateUpdate != null)
            {
                OnLateUpdate();
            }
        }

        public void ExecuteOnClose()
        {
            if (OnClose != null)
            {
                OnClose();
            }
        }

        public void ExecuteOnDrawGizmos()
        {
            if (OnDrawGizmos != null)
            {
                OnDrawGizmos();
            }
        }
    }
}
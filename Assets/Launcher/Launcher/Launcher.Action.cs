using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HybridCLR;

namespace LccModel
{
    public partial class Launcher
    {
        public Action ActionFixedUpdate;
        public Action ActionUpdate;
        public Action ActionLateUpdate;
        public Action ActionClose;
        public Action ActionOnDrawGizmos;

        public void FixedUpdate()
        {
            ActionFixedUpdate?.Invoke();
        }

        public void Update()
        {
            ActionUpdate?.Invoke();
        }

        public void LateUpdate()
        {
            ActionLateUpdate?.Invoke();
        }

        public void OnApplicationQuit()
        {
            ActionClose?.Invoke();
        }

        public void OnDrawGizmos()
        {
            ActionOnDrawGizmos?.Invoke();
        }
    }

}
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HybridCLR;

namespace LccModel
{
    public partial class Launcher
    {
        public Action actionFixedUpdate;
        public Action actionUpdate;
        public Action actionLateUpdate;
        public Action actionClose;
        public Action actionOnDrawGizmos;

        public void FixedUpdate()
        {
            actionFixedUpdate?.Invoke();
        }

        public void Update()
        {
            actionUpdate?.Invoke();
        }

        public void LateUpdate()
        {
            actionLateUpdate?.Invoke();
        }

        public void OnApplicationQuit()
        {
            actionClose?.Invoke();
        }

        public void OnDrawGizmos()
        {
            actionOnDrawGizmos?.Invoke();
        }
    }

}
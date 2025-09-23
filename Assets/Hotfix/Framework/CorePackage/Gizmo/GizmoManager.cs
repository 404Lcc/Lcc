using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class GizmoManager : Module, IGizmoService
    {
        public List<Action> gizmoActions = new List<Action>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            gizmoActions.Clear();
        }

        public void AddGizmo(Action action)
        {
            if (action == null)
                return;
            
            gizmoActions.Add(action);
        }

        public void RemoveGizmo(Action action)
        {
            if (action == null)
                return;
            
            gizmoActions.Remove(action);
        }

        public void OnDrawGizmos()
        {
            if (gizmoActions == null)
                return;

            foreach (var action in gizmoActions.ToArray())
            {
                action?.Invoke();
            }
        }
    }
}
using System;

namespace LccHotfix
{
    public interface IGizmoService : IService
    {
        void AddGizmo(Action action);

        void RemoveGizmo(Action action);

        void OnDrawGizmos();
    }
}
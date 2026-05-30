using System;
using UnityEngine;

namespace LccHotfix
{
    public struct EvtSubObjDestroy
    {
        public Vector3 Where;
    }

    public interface ISubobjectLifecycleEventService
    {
        void AddDestroyHandler(Action<EvtSubObjDestroy> handler);
        void RemoveDestroyHandler(Action<EvtSubObjDestroy> handler);
        void DispatchDestroy(EvtSubObjDestroy evt);
    }

    public sealed class SubobjectLifecycleEventService : ISubobjectLifecycleEventService
    {
        private event Action<EvtSubObjDestroy> _onDestroy;

        public void AddDestroyHandler(Action<EvtSubObjDestroy> handler)
        {
            _onDestroy += handler;
        }

        public void RemoveDestroyHandler(Action<EvtSubObjDestroy> handler)
        {
            _onDestroy -= handler;
        }

        public void DispatchDestroy(EvtSubObjDestroy evt)
        {
            _onDestroy?.Invoke(evt);
        }
    }

    public partial class LogicWorld
    {
        public ISubobjectLifecycleEventService SubobjectLifecycleEventService { get; private set; } = new SubobjectLifecycleEventService();

        public void SetSubobjectLifecycleEventService(ISubobjectLifecycleEventService service)
        {
            SubobjectLifecycleEventService = service ?? new SubobjectLifecycleEventService();
        }
    }
}

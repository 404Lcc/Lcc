using UnityEngine;

namespace LccHotfix
{
    public class MainUIView : IViewWrapper
    {
        //////////////////////////////////////////////////////////////////////////
        // IViewWrapper:
        public int Category { get; private set; }

        public string ViewName { get; set; }

        /// <summary>
        /// 绑定 Category；loaded/world 由子类按需使用。
        /// </summary>
        public virtual void Bind(IReceiveLoaded loaded, int category, ECWorlds world)
        {
            Category = category;
        }

        public virtual void Init(long entityId, IViewLoader loader, IViewWrapper parent)
        {
        }

        public virtual void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
        }

        public virtual void ModifyVisible(bool visible, int flag)
        {
        }

        public virtual void RemoveVisible(int flag)
        {
        }

        public virtual void DisposeView()
        {
            ViewName = null;
        }
    }
}

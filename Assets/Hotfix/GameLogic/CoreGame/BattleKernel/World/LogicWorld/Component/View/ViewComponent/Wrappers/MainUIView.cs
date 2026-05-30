using UnityEngine;

namespace LccHotfix
{
    public class MainUIView : IViewWrapper
    {
        public int Category { get; }
        public string ViewName { get; set; }

        public MainUIView(int category)
        {
            Category = category;
        }

        public void Init(long entityId, IViewLoader loader, IViewWrapper parent)
        {
        }

        public void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
        }

        public void ModifyVisible(bool visible, int flag)
        {
        }

        public void RemoveVisible(int flag)
        {
        }

        public void DisposeView()
        {
        }
    }
}
using UnityEngine;

namespace LccHotfix
{
    public class RecordSubView : IViewWrapper
    {
        protected GameObjectHandle _poolobj;
        public int Category { get; set; }
        public string ViewName { get; set; }

        public RecordSubView(GameObjectHandle poolobj, int category) //EViewCategory.AddedGameObject
        {
            _poolobj = poolobj;
            Category = category;
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
            if (_poolobj != null)
            {
                _poolobj.Transform.SetParent(null);
                _poolobj.Release(ref _poolobj);
                _poolobj = null;
            }
        }
    }
}
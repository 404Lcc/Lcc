using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// 测试最简 View
    public class SimpleGameObjectView : IViewWrapper
    {
        protected ObjReceiveLoaded MLoader;

        public GameObject GameObject
        {
            get { return MLoader.GetHandle().GameObject; }
        }

        protected Transform m_transform;

        public Transform Transform
        {
            get { return m_transform; }
        }

        public SimpleGameObjectView(ObjReceiveLoaded loader, int category)
        {
            MLoader = loader;
            m_transform = MLoader.GetHandle().Transform;
            Category = category;
            IsVisible = new MultChangeBool_AND(true);
        }

        public int Category { get; private set; }

        public string ViewName { get; set; }
        public string BindPointName { get; set; }

        public bool ClearFx { get; set; }

        public MultChangeBool_AND IsVisible { get; set; }

        private Dictionary<string, Transform> mBpName2Transform = new Dictionary<string, Transform>();

        private Material _originalMaterial;

        public void ReplaceMaterial(Material newMat)
        {
            if (GameObject == null)
                return;

            var renderers = GameObject.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.gameObject.name.StartsWith("Hit")) // 忽略受击效果遮罩
                    continue;
                if (_originalMaterial == null && r.sharedMaterial != null)
                    _originalMaterial = r.sharedMaterial;

                r.sharedMaterial = newMat;
            }
        }

        public void RestoreMaterial()
        {
            if (_originalMaterial == null)
                return;

            var renderers = GameObject.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.gameObject.name.StartsWith("Hit"))
                    continue;
                r.sharedMaterial = _originalMaterial;
            }
        }

        public virtual void DisposeView()
        {
            RestoreMaterial();
            if (MLoader.GetHandle() == null)
            {
                UnityEngine.Debug.LogError($"DisposeView m_gameObject == null: ViewName={ViewName}");
            }

            MLoader.Dispose();
            // m_gameObject = null;
            mBpName2Transform.Clear();
        }

        public virtual void Init(long entityId, IViewLoader loader, IViewWrapper parent)
        {

        }

        public virtual void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            m_transform.position = position;
            m_transform.rotation = rotation;
            m_transform.localScale = scale;
        }

        public bool HasBindPoint(string bpName)
        {
            return mBpName2Transform.ContainsKey(bpName);
        }

        public Transform GetBindPoint(string bpName)
        {
            if (mBpName2Transform.TryGetValue(bpName, out var value))
            {
                return value;
            }

            var bpTrans = ModelBindPointGetter.GetBindPoint(Transform, bpName);
            if (bpTrans == null)
                return Transform;
            SetBindPoint(bpName, bpTrans);
            return bpTrans;
        }

        public void SetBindPoint(string bpName, Transform bpTrans)
        {
            mBpName2Transform[bpName] = bpTrans;
        }


        public void ModifyVisible(bool visible, int flag)
        {
            IsVisible.AddChange(visible, flag);
            if (MLoader.GetHandle().IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }

        public void RemoveVisible(int flag)
        {
            IsVisible.RemoveChange(flag);
            if (MLoader.GetHandle().IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }
    }
}
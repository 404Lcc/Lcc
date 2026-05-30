using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class MainGameObjectView : IViewWrapper
    {
        protected ObjReceiveLoaded _loader;

        public GameObject GameObject
        {
            get { return _loader.GetHandle().GameObject; }
        }

        protected Transform _transform;

        public Transform Transform
        {
            get { return _transform; }
        }

        public MainGameObjectView(ObjReceiveLoaded loader, int category)
        {
            _loader = loader;
            _transform = _loader.GetHandle().Transform;
            Category = category;
            IsVisible = new MultChangeBool_AND(true);
        }

        public int Category { get; private set; }

        public string ViewName { get; set; }
        public string BindPointName { get; set; }

        public bool ClearFx { get; set; }

        public MultChangeBool_AND IsVisible { get; set; }

        private Dictionary<string, Transform> _bpName2Transform = new Dictionary<string, Transform>();

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
            if (_loader.GetHandle() == null)
            {
                UnityEngine.Debug.LogError($"DisposeView gameObject == null: ViewName={ViewName}");
            }

            _loader.Dispose();
            _bpName2Transform.Clear();
        }

        public virtual void Init(long entityId, IViewLoader loader, IViewWrapper parent)
        {
        }

        public virtual void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _transform.position = position;
            _transform.rotation = rotation;
            _transform.localScale = scale;
        }

        public bool HasBindPoint(string bpName)
        {
            return _bpName2Transform.ContainsKey(bpName);
        }

        public Transform GetBindPoint(string bpName)
        {
            if (_bpName2Transform.TryGetValue(bpName, out var value))
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
            _bpName2Transform[bpName] = bpTrans;
        }


        public void ModifyVisible(bool visible, int flag)
        {
            IsVisible.AddChange(visible, flag);
            if (_loader.GetHandle().IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }

        public void RemoveVisible(int flag)
        {
            IsVisible.RemoveChange(flag);
            if (_loader.GetHandle().IsDone)
            {
                GameObject.SetActive(IsVisible.Value);
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class MainGameObjectView : IViewWrapper
    {
        private static readonly int MaterialParam_BuffIndex = Shader.PropertyToID("_Index");
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

        public string BindPointName { get; set; }

        public bool ClearFx { get; set; }

        public MultChangeBool_AND IsVisible { get; set; }

        private Dictionary<string, Transform> _bpName2Transform = new Dictionary<string, Transform>();
        private HashSet<Transform> _materialExcludeRoots;

        //////////////////////////////////////////////////////////////////////////
        // IViewWrapper:
        public int Category { get; private set; }

        public string ViewName { get; set; }

        /// <summary>
        /// 绑定 ObjReceiveLoaded 与 Category，重置显隐；可被池化复用。
        /// </summary>
        public virtual void Bind(IReceiveLoaded loaded, int category, ECWorlds world)
        {
            Category = category;
            if (loaded is ObjReceiveLoaded objLoaded)
            {
                _loader = objLoaded;
                var handle = _loader.GetHandle();
                _transform = handle != null ? handle.Transform : null;
            }

            if (IsVisible == null)
            {
                IsVisible = new MultChangeBool_AND(true);
            }
            else
            {
                IsVisible.Clear();
                IsVisible.DefaultValue = true;
            }
        }

        public virtual void Init(long entityId, IViewLoader loader, IViewWrapper parent)
        {
            // ObjName 与对象池资源名一致，引用赋值无 GC，供挂点查找使用
            if (loader is ObjViewLoader objLoader)
                ViewName = objLoader.ObjName;
        }

        public virtual void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _transform.position = position;
            _transform.rotation = rotation;
            _transform.localScale = scale;
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

        public virtual void DisposeView()
        {
            RestoreMaterial();
            if (_loader.GetHandle() == null)
            {
                UnityEngine.Debug.LogError($"DisposeView gameObject == null: ViewName={ViewName}");
            }

            _loader.Dispose();
            _loader = default;
            _transform = null;
            ViewName = null;
            BindPointName = null;
            ClearFx = false;
            _bpName2Transform.Clear();
            _materialExcludeRoots = null;
            if (IsVisible != null)
            {
                IsVisible.Clear();
                IsVisible.DefaultValue = true;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // MainGameObjectView:
        /// <summary>
        /// 注册不参与 ReplaceMaterial 的子树根节点（如电击 Bone）。动态挂载/释放时成对调用。
        /// </summary>
        public void RegisterMaterialExcludeRoot(Transform root)
        {
            if (root == null)
                return;

            _materialExcludeRoots ??= new HashSet<Transform>();
            _materialExcludeRoots.Add(root);
        }

        public void UnregisterMaterialExcludeRoot(Transform root)
        {
            if (root == null || _materialExcludeRoots == null)
                return;

            _materialExcludeRoots.Remove(root);
            if (_materialExcludeRoots.Count == 0)
                _materialExcludeRoots = null;
        }

        private bool ShouldSkipMaterialRenderer(Transform rendererTransform)
        {
            if (_materialExcludeRoots == null || rendererTransform == null)
                return false;

            foreach (var root in _materialExcludeRoots)
            {
                if (root != null && rendererTransform.IsChildOf(root))
                    return true;
            }

            return false;
        }

        public void ReplaceMaterial(int index)
        {
            if (GameObject == null)
                return;

            var renderers = GameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var r in renderers)
            {
                if (r == null || ShouldSkipMaterialRenderer(r.transform))
                    continue;

                r.material.SetFloat(MaterialParam_BuffIndex, index);
            }
        }

        public void RestoreMaterial()
        {
            ReplaceMaterial(0);
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

            // ViewName 未写入时只读一次 Transform.name，避免查找热路径反复分配
            if (string.IsNullOrEmpty(ViewName) && Transform != null)
                ViewName = Transform.name;

            var bpTrans = ModelBindPointGetter.GetBindPoint(Transform, ViewName, bpName);
            if (bpTrans == null)
                return Transform;
            SetBindPoint(bpName, bpTrans);
            return bpTrans;
        }

        public void SetBindPoint(string bpName, Transform bpTrans)
        {
            _bpName2Transform[bpName] = bpTrans;
        }
    }
}

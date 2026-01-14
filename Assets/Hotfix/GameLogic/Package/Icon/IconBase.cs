using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class IconBase : IReference
    {
        private Transform _parent;
        private float _size;
        private object[] _args;

        private GameObjectHandle _handle;

        public GameObject GameObject => _handle.GameObject;
        public Transform Transform => _handle.Transform;
        public bool IsDone => _handle.IsDone;

        public void InitIcon(Transform parent, float size)
        {
            this._parent = parent;
            _size = size;

            _handle = Main.GameObjectPoolService.GetObjectAsync(GetType().Name, OnComplete);
        }

        private void OnComplete(GameObjectHandle obj)
        {
            ClientTools.ResetTransform(Transform, _parent);
            ClientTools.ResetRectTransform(Transform as RectTransform);
            SetSize(_size);

            ClientTools.AutoReference(Transform, this);

            OnShow();
            if (_args != null)
            {
                UpdateData(_args);
            }
        }

        public void OnRecycle()
        {
            if (IsDone)
            {
                OnHide();
            }

            _args = null;
            _handle.Release(ref _handle);
        }

        /// <summary>
        /// go加载出来之后调用（可以在这里add事件）
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// 更新数据（每次SetInfo会触发）
        /// </summary>
        /// <param name="info"></param>
        protected virtual void UpdateData(object[] args)
        {
        }

        /// <summary>
        /// 如果OnShow调用过，则Release的时候会触发（可以在这里remove事件）
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="info"></param>
        public void SetInfo(params object[] args)
        {
            _args = args;
            if (IsDone)
            {
                UpdateData(args);
            }
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Release<T>(ref T iconBase) where T : IconBase
        {
            if (iconBase == null)
                return;

            ReferencePool.Release(iconBase);
            iconBase = null;
        }

        public void SetIcon(UIImageCtrl iconImage, int newImageID)
        {
            iconImage.SetImage(newImageID);
        }

        public void SetSize(float size)
        {
            if (_handle == null || !_handle.IsDone)
            {
                return;
            }

            var scale = Vector3.one * size;
            GameObject.transform.localScale = scale;
        }
    }
}
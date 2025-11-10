using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class IconBase : IReference
    {
        private Transform _parent;
        private float _size;
        private object _info;

        private GameObjectPoolAsyncOperation _asyncOperation;

        public GameObject GameObject => _asyncOperation.GameObject;
        public Transform Transform => _asyncOperation.Transform;
        public bool IsDone => _asyncOperation.IsDone;

        public void InitIcon(Transform parent, float size, object info = null)
        {
            this._parent = parent;
            _size = size;
            _info = info;

            _asyncOperation = Main.GameObjectPoolService.GetObjectAsync(GetType().Name, OnComplete);
        }

        private void OnComplete(GameObjectPoolAsyncOperation obj)
        {
            _asyncOperation = obj;
            ClientTools.ResetTransform(Transform, _parent);
            ClientTools.ResetRectTransform(Transform as RectTransform);
            SetSize(_size);

            ClientTools.AutoReference(Transform, this);

            OnShow();
        }

        public void OnRecycle()
        {
            if (IsDone)
            {
                OnHide();
            }

            _info = null;
            _asyncOperation.Release(ref _asyncOperation);
        }

        /// <summary>
        /// go加载出来之后调用（可以在这里add事件）
        /// </summary>
        protected virtual void OnShow()
        {
            if (_info != null)
            {
                UpdateData(_info);
            }
        }

        /// <summary>
        /// 更新数据（每次SetInfo会触发）
        /// </summary>
        /// <param name="info"></param>
        protected virtual void UpdateData(object info)
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
        public void SetInfo(object info)
        {
            _info = info;

            if (IsDone)
            {
                UpdateData(info);
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

        public void SetIcon(UIImageCtrl image, int newImageID)
        {
            image.SetImage(newImageID);
        }

        public void SetSize(float size)
        {
            if (_asyncOperation == null || !_asyncOperation.IsDone)
            {
                return;
            }

            var scale = Vector3.one * size;
            GameObject.transform.localScale = scale;
        }
    }
}
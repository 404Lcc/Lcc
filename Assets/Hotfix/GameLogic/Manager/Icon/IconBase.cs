using System;
using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class IconBase : IReference
    {
        private Transform _parent;
        private float _size;

        private GameObjectPoolAsyncOperation _asyncOperation;

        public GameObject GameObject => _asyncOperation.GameObject;
        public Transform Transform => _asyncOperation.Transform;

        public void InitIcon(Transform parent, float size)
        {
            this._parent = parent;
            _size = size;

            _asyncOperation = Main.GameObjectPoolService.GetObjectAsync(GetType().Name, OnComplete);
        }

        private void OnComplete(GameObjectPoolAsyncOperation obj)
        {
            _asyncOperation = obj;
            ClientTools.ResetTransform(Transform, _parent);
            ClientTools.ResetRectTransform(Transform as RectTransform);
            SetSize(_size);

            ClientTools.AutoReference(Transform, this);

            OnInit();
        }

        public virtual void OnInit()
        {
        }

        public void OnRecycle()
        {
            OnReset(_asyncOperation.IsDone);

            _asyncOperation.Release(ref _asyncOperation);
        }

        protected virtual void OnReset(bool isDone)
        {
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
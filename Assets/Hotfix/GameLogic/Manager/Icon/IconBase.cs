using System;
using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class IconBase : IReference
    {
        private IconType _iconType;
        private Transform _parent;
        private IconSize _size;

        private GameObjectPoolAsyncOperation _asyncOperation;
        private bool _clickShowTips;
        private Action _onClick;
        private UIImageCtrl _iconImage;

        public GameObject GameObject => _asyncOperation.GameObject;
        public Transform Transform => _asyncOperation.Transform;

        public void InitIcon(IconType iconType, Transform parent, IconSize size)
        {
            this._iconType = iconType;
            this._parent = parent;
            _size = size;

            _asyncOperation = Main.GameObjectPoolService.GetObjectAsync(iconType.ToString(), OnComplete);
        }

        private void OnComplete(GameObjectPoolAsyncOperation obj)
        {
            ClientTools.ResetTransform(Transform, _parent);
            ClientTools.ResetRectTransfrom(Transform as RectTransform);
            SetSize(_size);

            ClientTools.AutoReference(Transform, this);
            ClientTools.ForceGetComponent<Button>(GameObject).onClick.AddListener(OnClick);

            OnInit();
        }

        public virtual void OnInit()
        {
        }

        public void OnRecycle()
        {
            _onClick = null;
            _clickShowTips = true;

            OnReset();

            _asyncOperation.Release(ref _asyncOperation);
        }

        protected virtual void OnShowClickTips()
        {
        }

        protected virtual void OnReset()
        {
        }

        private void OnClick()
        {
            _onClick?.Invoke();

            if (_clickShowTips)
            {
                OnShowClickTips();
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

        public virtual void SetInfo(int newImageID)
        {
            SetIcon(newImageID);
        }

        public virtual void SetInfo(int newImageID, long count)
        {
            SetIcon(newImageID);
        }

        public virtual void SetIcon(int newImageID)
        {
            _iconImage.SetImage(newImageID);
        }

        public void SetClickShowTips(bool clickShowTips)
        {
            _clickShowTips = clickShowTips;
        }

        public void SetClick(Action action)
        {
            _onClick = action;
        }

        public void SetSize(IconSize size = IconSize.Size_100)
        {
            if (_asyncOperation == null || !_asyncOperation.IsDone)
            {
                return;
            }

            var scale = Main.IconService.GetIconScale(size);
            GameObject.transform.localScale = scale;
        }
    }
}
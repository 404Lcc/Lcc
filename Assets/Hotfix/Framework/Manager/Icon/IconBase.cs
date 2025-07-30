using System;
using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class IconBase : IReference
    {
        private IconType _iconType;
        private bool _clickShowTips;

        private GameObject _gameObject;
        private Action _onClick;

        private UIImageCtrl _iconImage;

        public GameObject GameObject => _gameObject;

        public void InitIcon(GameObject gameObject, IconType iconType)
        {
            this._gameObject = gameObject;
            this._iconType = iconType;
            ClientTools.AutoReference(gameObject.transform, this);
            ClientTools.ForceGetComponent<Button>(gameObject).onClick.AddListener(OnClick);

            OnInit();
        }

        public virtual void OnInit()
        {

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

        public virtual void ShowClickTips()
        {

        }

        public virtual void OnReset()
        {

        }

        public void SetClickShowTips(bool clickShowTips)
        {
            _clickShowTips = clickShowTips;
        }
        
        public void SetClick(Action action)
        {
            _onClick = action;
        }



        // 0: 没有流光 1: icon流光 2：背景框流光 3：icon+背景框流光
        public void SetIconFlow(int iconEffect)
        {
            if (iconEffect == 1 || iconEffect == 3)
            {
                if (_iconImage != null)
                {
                    _iconImage.SpriteCtrl.SetFlow(true);
                }
            }
            else if (_iconImage != null)
            {
                _iconImage.SpriteCtrl.SetFlow(false);
            }
        }

        public void SetGray(bool toGray)
        {
            _iconImage.SpriteCtrl.SetGray(toGray);
        }

        public void SetSize(IconSize size = IconSize.Size_100)
        {
            var scale = IconManager.Instance.GetIconScale(size);
            _gameObject.transform.localScale = scale;
        }


        public void OnClick()
        {
            _onClick?.Invoke();

            if (_clickShowTips)
            {
                ShowClickTips();
            }
        }


        public void Clear()
        {
            //重置点击
            //重置置灰

            SetClick(null);
            SetClickShowTips(true);
            SetIconFlow(0);
            SetGray(false);

            OnReset();
        }
    }
}
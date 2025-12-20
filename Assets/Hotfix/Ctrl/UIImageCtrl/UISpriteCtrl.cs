using System.Collections.Generic;
using UnityEngine;
using System;
using LccModel;
using UnityEngine.UI;
using YooAsset;

namespace LccHotfix
{
    [RequireComponent(typeof(Image))]
    public class UISpriteCtrl : MonoBehaviour
    {
        public Action onLoadDone;
        public string spriteName;
        [Tooltip("使用本地化图集")]
        public bool localSprite;
        [Tooltip("适应图片大小")]
        public bool perfectSpirte;

        private bool _isStarted = false;//被复制时不会再走一次start

        private Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();
        private Image _image;

        public Image CacheImage
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }

        private void Awake()
        {
            _image = null;
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (GetComponents<UISpriteCtrl>().Length > 1)
            {
                Debug.LogError("there are more than one UISpriteCtrl on the same object：" + gameObject.name);
            }
#endif
            if (_isStarted)
                return;
            _isStarted = true;
            if (localSprite)
                OnLocalize();
            else
                SetSprite();
        }

        public void OnLocalize()
        {
            if (!localSprite) return;
            if (string.IsNullOrEmpty(spriteName)) return;

            if (CacheImage == null) return;

            string lang = null;
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                lang = Launcher.Instance.GameLanguage.curLanguage;
            }
            else
            {
                lang = "English";
            }
#else
            lang = Launcher.Instance.GameLanguage.curLanguage;
#endif
            if (string.IsNullOrEmpty(lang))
            {
                Debug.LogError("storm not inited");
                return;
            }
            int index = spriteName.LastIndexOf('_');
            if (index < 0) return;
            spriteName = spriteName.Substring(0, index + 1);

            SetSpriteName(spriteName + lang);
        }


        public void SetSpriteName(string newSpriteName)
        {
            if (string.IsNullOrEmpty(newSpriteName) || spriteName == newSpriteName) return;
            _isStarted = true;
            spriteName = newSpriteName;

            if (localSprite)
            {
                string lang = Launcher.Instance.GameLanguage.curLanguage;
                if (!string.IsNullOrEmpty(lang))
                {
                    int index = spriteName.LastIndexOf('_');
                    if (index >= 0)
                    {
                        spriteName = spriteName.Substring(0, index + 1);
                        spriteName += lang;
                    }
                }
            }

            SetSprite();
        }

        private void SetSprite()
        {
            if (CacheImage == null)
                return;
            if (string.IsNullOrEmpty(spriteName))
                return;

            if (CacheImage.sprite != null && CacheImage.sprite.name.Equals(spriteName))
            {
                return;
            }
            if (_spriteDict.ContainsKey(spriteName))
            {
                CacheImage.sprite = _spriteDict[spriteName];
                SetPerfect();
            }
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return;
                }
#endif
                Main.AssetService.LoadAssetAsync<Sprite>(spriteName, LoadSpriteDone);
            }
        }

        void LoadSpriteDone(AssetHandle assetHandle)
        {
            if (assetHandle == null)
            {
                return;
            }
            try
            {
                var sprite = assetHandle.AssetObject as Sprite;
                if (sprite == null)
                    return;
                string loadSpriteName = sprite.name;
                if (!_spriteDict.ContainsKey(loadSpriteName))
                    _spriteDict.Add(loadSpriteName, sprite);

                if (!loadSpriteName.Equals(spriteName) ||//图集不对
                    (CacheImage.sprite != null && CacheImage.sprite.name.Equals(spriteName)))//跟现在的图集相同
                {
                    return;
                }

                CacheImage.sprite = sprite;
                SetPerfect();
                if (onLoadDone != null)
                    onLoadDone();
            }
            catch (System.Exception ex)
            {
            }
        }

        private void SetPerfect()
        {
            if (perfectSpirte && CacheImage != null)
            {
                CacheImage.SetNativeSize();
            }
        }
    }
}
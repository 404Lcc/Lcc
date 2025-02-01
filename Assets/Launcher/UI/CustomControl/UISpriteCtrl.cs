using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace LccModel
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

        public bool isGray;//灰色图集
        public bool isFlow;//流光图集

        private bool _isStarted = false;//被复制时不会再走一次start

        private Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();
        private Image _image;

        private Material _material;

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

        public Material CacheMaterial
        {
            get
            {
                if (_material == null)
                {
                    _material = ResObject.LoadRes<Material>(gameObject, "Actor_RGBA_Flow").GetAsset<Material>();
                }
                return _material;
            }
        }

        private void Awake()
        {
            _image = null;
            _material = null;
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
                lang = Launcher.Instance.curLanguage;
            }
            else
            {
                lang = "English";
            }
#else
            lang = Launcher.Instance.curLanguage;
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
                string lang = Launcher.Instance.curLanguage;
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
                ResObject.StartLoadRes<GameObject>(gameObject, spriteName, LoadSpriteDone);
            }
        }

        void LoadSpriteDone(string assetName, UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return;
            }
            try
            {
                var sprite = obj as Sprite;
                if (sprite == null) return;
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
                Debug.LogError("load atlas failed : " + assetName + " with error: " + ex);
            }
        }

        public void SetGray(bool toGray)
        {
            isGray = toGray;
            CacheMaterial.SetFloat("_IsGray", isGray ? 1 : 0);
        }
        public void SetFlow(bool toFlow)
        {
            isFlow = toFlow;
            CacheMaterial.SetFloat("_IsFlow", isFlow ? 1 : 0);
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
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    [RequireComponent(typeof(Image))]
    public class UISpriteCtrl : MonoBehaviour
    {
        private string _spriteName;
        private Action<Sprite> _callback;

        [Tooltip("使用本地化Sprite")] public bool localSprite;

        public void GetSprite(string spriteName, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(spriteName))
                return;

            if (localSprite)
            {
                //todo 默认用英语
                string lang = "English";

                int index = spriteName.LastIndexOf('_');
                if (index < 0)
                {
                    Debug.LogError("不是多语言图");
                    return;
                }

                spriteName = spriteName.Substring(0, index + 1);
                spriteName += lang;
            }

            _spriteName = spriteName;
            _callback = callback;

            IconUtility.GetSprite(_spriteName, LoadSpriteDone);
        }

        void LoadSpriteDone(Sprite sprite)
        {
            if (string.IsNullOrEmpty(_spriteName))
            {
                return;
            }

            if (sprite == null)
            {
                return;
            }

            try
            {
                string loadSpriteName = sprite.name;
                if (!loadSpriteName.Equals(_spriteName))
                {
                    return;
                }

                _callback?.Invoke(sprite);

                _spriteName = null;
                _callback = null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("load sprite failed : " + ex);
            }
        }

        public void OnDestroy()
        {
            _spriteName = null;
            _callback = null;
        }
    }
}
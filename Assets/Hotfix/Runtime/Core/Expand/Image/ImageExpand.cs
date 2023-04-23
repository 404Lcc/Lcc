using BM;
using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public static class ImageExpand
    {
        public static LoadHandler SetSprite(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Sprite sprite = AssetManager.Instance.LoadAsset<Sprite>(out LoadHandler handler, name, AssetSuffix.Png, types);
            image.sprite = sprite;
            return handler;
        }
    }
}
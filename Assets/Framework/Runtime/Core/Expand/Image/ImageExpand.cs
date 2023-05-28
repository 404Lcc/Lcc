using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LccModel
{
    public static class ImageExpand
    {
        public static AssetOperationHandle SetSprite(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Sprite sprite = AssetManager.Instance.LoadAsset<Sprite>(out AssetOperationHandle handle, name, AssetSuffix.Png, types);
            image.sprite = sprite;
            return handle;
        }
    }
}
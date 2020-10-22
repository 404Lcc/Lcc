using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public static class ImageExpand
    {
        public static void SetSprite(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = LccModel.AssetManager.Instance.LoadAssetData<Sprite>(name, ".png", false, true, types);
            image.sprite = sprite;
        }
    }
}
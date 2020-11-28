using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public static class ImageExpand
    {
        public static async void SetSprite(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = await LccModel.AssetManager.Instance.LoadAssetAsync<Sprite>(name, ".png", false, true, types);
            image.sprite = sprite;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public static class ImageExpand
    {
        public static async void SetSpriteAsync(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = await AssetManager.Instance.LoadAssetAsync<Sprite>(name, ".png", false, true, types);
            image.sprite = sprite;
        }
    }
}
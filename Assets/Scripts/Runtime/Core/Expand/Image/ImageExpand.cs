using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public static class ImageExpand
    {
        public async static void SetSprite(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = await AssetManager.Instance.LoadAsset<Sprite>(name, ".png", false, true, types);
            image.sprite = sprite;
        }
    }
}
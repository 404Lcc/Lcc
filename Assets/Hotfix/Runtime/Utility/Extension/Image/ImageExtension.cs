using LccModel;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LccHotfix
{
    public static class ImageExtension
    {
        public static void SetSprite(this Image image, string location)
        {
            if (string.IsNullOrEmpty(location))
                return;
            Sprite sprite = AssetManager.Instance.LoadRes<Sprite>(image.gameObject, location);
            image.sprite = sprite;
        }
    }
}
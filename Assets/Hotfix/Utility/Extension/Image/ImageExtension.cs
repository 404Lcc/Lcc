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
            Main.AssetService.LoadRes<Sprite>(image.gameObject, location, out var res);
            image.sprite = res;
        }
    }
}
using LccModel;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public static class SpriteRendererExtension
    {
        public static void SetSprite(this SpriteRenderer spriteRenderer, string location)
        {
            if (string.IsNullOrEmpty(location))
                return;
            Main.AssetService.LoadRes<Sprite>(spriteRenderer.gameObject, location, out var res);
            spriteRenderer.sprite = res;
        }
    }
}
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
            Sprite sprite = AssetManager.Instance.LoadRes<Sprite>(spriteRenderer.gameObject, location);
            spriteRenderer.sprite = sprite;
        }
    }
}
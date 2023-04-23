using BM;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public static class SpriteRendererExpand
    {
        public static LoadHandler SetSprite(this SpriteRenderer spriteRenderer, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Sprite sprite = AssetManager.Instance.LoadAsset<Sprite>(out LoadHandler handler, name, AssetSuffix.Png, types);
            spriteRenderer.sprite = sprite;
            return handler;
        }
    }
}
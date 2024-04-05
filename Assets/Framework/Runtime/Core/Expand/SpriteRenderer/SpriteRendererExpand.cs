using UnityEngine;
using YooAsset;

namespace LccModel
{
    public static class SpriteRendererExpand
    {
        public static AssetHandle SetSprite(this SpriteRenderer spriteRenderer, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Sprite sprite = AssetManager.Instance.LoadAsset<Sprite>(out AssetHandle handle, name, AssetSuffix.Png, types);
            spriteRenderer.sprite = sprite;
            return handle;
        }
    }
}
using UnityEngine;

namespace LccModel
{
    public static class SpriteRendererExpand
    {
        public static async void SetSpriteAsync(this SpriteRenderer spriteRenderer, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = await AssetManager.Instance.LoadAssetAsync<Sprite>(name, ".png", false, true, types);
            spriteRenderer.sprite = sprite;
        }
    }
}
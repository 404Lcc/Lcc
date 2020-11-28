using UnityEngine;

namespace LccHotfix
{
    public static class SpriteRendererExpand
    {
        public static async void SetSprite(this SpriteRenderer spriteRenderer, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = await LccModel.AssetManager.Instance.LoadAssetAsync<Sprite>(name, ".png", false, true, types);
            spriteRenderer.sprite = sprite;
        }
    }
}
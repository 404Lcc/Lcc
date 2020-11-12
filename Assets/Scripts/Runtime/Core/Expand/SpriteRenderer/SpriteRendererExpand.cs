using UnityEngine;

namespace LccModel
{
    public static class SpriteRendererExpand
    {
        public async static void SetSprite(this SpriteRenderer spriteRenderer, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = await AssetManager.Instance.LoadAsset<Sprite>(name, ".png", false, true, types);
            spriteRenderer.sprite = sprite;
        }
    }
}
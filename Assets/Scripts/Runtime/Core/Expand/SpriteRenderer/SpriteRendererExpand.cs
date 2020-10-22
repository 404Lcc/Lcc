using UnityEngine;

namespace LccModel
{
    public static class SpriteRendererExpand
    {
        public static void SetSprite(this SpriteRenderer spriteRenderer, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = AssetManager.Instance.LoadAssetData<Sprite>(name, ".png", false, true, types);
            spriteRenderer.sprite = sprite;
        }
    }
}
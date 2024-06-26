﻿using UnityEngine;
using YooAsset;

namespace LccModel
{
    public static class SpriteRendererExpand
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
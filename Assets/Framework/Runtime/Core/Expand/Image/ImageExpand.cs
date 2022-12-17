﻿using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public static class ImageExpand
    {
        public static void SetSprite(this Image image, string name, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = AssetManager.Instance.LoadAsset<Sprite>(name, AssetSuffix.Png, types);
            image.sprite = sprite;
        }
    }
}
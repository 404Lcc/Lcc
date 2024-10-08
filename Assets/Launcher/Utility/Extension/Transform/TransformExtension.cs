﻿using UnityEngine;

namespace LccModel
{
    public static class TransformExtension
    {
        public static Vector2 SizeDelta(this RectTransform rectTransform)
        {
            Vector2 size = new Vector2(rectTransform.rect.size.x, rectTransform.rect.size.y);

            return size;
        }
    }
}
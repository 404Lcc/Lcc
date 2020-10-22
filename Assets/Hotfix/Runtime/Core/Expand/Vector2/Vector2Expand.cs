using UnityEngine;

namespace LccHotfix
{
    public static class Vector2Expand
    {
        public static Vector2 ScreenToUGUI(this Vector2 screenPoint, RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, null, out Vector2 localPosition);
            return localPosition;
        }
    }
}
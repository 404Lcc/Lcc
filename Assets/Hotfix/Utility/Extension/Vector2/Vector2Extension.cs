using UnityEngine;

namespace LccHotfix
{
    public static class Vector2Extension
    {
        public static Vector2 ScreenToUGUI(this Vector2 screenPoint, Camera uiCamera, RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, uiCamera, out Vector2 localPosition);
            return localPosition;
        }
    }
}
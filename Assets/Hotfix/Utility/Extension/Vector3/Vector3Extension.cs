using UnityEngine;

namespace LccHotfix
{
    public static class Vector3Extension
    {
        public static Vector3 ScreenToUGUI(this Vector3 screenPoint, Camera uiCamera, RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, uiCamera, out Vector2 localPosition);
            return localPosition;
        }
    }
}
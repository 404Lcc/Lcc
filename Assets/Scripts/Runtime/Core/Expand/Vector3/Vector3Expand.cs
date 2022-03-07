using UnityEngine;

namespace LccModel
{
    public static class Vector3Expand
    {
        public static Vector3 ScreenToUGUI(this Vector3 screenPoint, RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, null, out Vector2 localPosition);
            return localPosition;
        }
    }
}
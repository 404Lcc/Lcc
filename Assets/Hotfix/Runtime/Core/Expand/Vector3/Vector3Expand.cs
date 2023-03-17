using UnityEngine;

namespace LccHotfix
{
    public static class Vector3Expand
    {
        public static Vector3 ScreenToUGUI(this Vector3 screenPoint, RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, GlobalManager.Instance.UICamera, out Vector2 localPosition);
            return localPosition;
        }
    }
}
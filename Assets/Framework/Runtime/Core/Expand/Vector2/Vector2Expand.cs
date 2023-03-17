using UnityEngine;

namespace LccModel
{
    public static class Vector2Expand
    {
        public static Vector2 ScreenToUGUI(this Vector2 screenPoint, RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, GameObject.Find("Global/UICamera").GetComponent<Camera>(), out Vector2 localPosition);
            return localPosition;
        }
    }
}
using UnityEngine;

namespace LccHotfix
{
    public static class ScreenAdaptationUtility
    {
        //开发屏幕的宽
        public static float DevelopWidth = 1920;
        //开发屏幕的高
        public static float DevelopHeigh = 1080;
        //开发高宽比
        public static float DevelopRate = DevelopHeigh / DevelopWidth;
        //设备自身的宽
        public static int curScreenWidth = Screen.width;
        //设备自身的高
        public static int curScreenHeight = Screen.height;
        //当前屏幕高宽比
        public static float ScreenRate = Screen.height / (float)Screen.width;

        //世界摄像机rect高的比例
        public static float cameraRectHeightRate = DevelopHeigh / ((DevelopWidth / Screen.width) * Screen.height);
        //世界摄像机rect宽的比例
        public static float cameraRectWidthRate = DevelopWidth / ((DevelopHeigh / Screen.height) * Screen.width);

        public static void CameraAdaptation(Camera camera)
        {
            //适配屏幕。实际屏幕比例<=开发比例的 上下黑  反之左右黑
            if (DevelopRate <= ScreenRate)
            {
                camera.rect = new Rect(0, (1 - cameraRectHeightRate) / 2, 1, cameraRectHeightRate);
            }
            else
            {
                camera.rect = new Rect((1 - cameraRectWidthRate) / 2, 0, cameraRectWidthRate, 1);
            }
        }
        public static void UIPanelAdaptation(GameObject gameObject)
        {
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            Vector2 anchorMin = Screen.safeArea.position;
            Vector2 anchorMax = Screen.safeArea.position + Screen.safeArea.size;
            anchorMin = new Vector2(anchorMin.x / Screen.width, anchorMin.y / Screen.height);
            anchorMax = new Vector2(anchorMax.x / Screen.width, anchorMax.y / Screen.height);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
        }
    }
}
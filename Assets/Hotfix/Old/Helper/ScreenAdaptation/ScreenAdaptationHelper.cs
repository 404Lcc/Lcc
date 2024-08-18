using UnityEngine;

public static class ScreenAdaptationHelper
{
    //������Ļ�Ŀ�
    public static float DevelopWidth = 1920;
    //������Ļ�ĸ�
    public static float DevelopHeigh = 1080;
    //�����߿��
    public static float DevelopRate = DevelopHeigh / DevelopWidth;
    //�豸����Ŀ�
    public static int curScreenWidth = Screen.width;
    //�豸����ĸ�
    public static int curScreenHeight = Screen.height;
    //��ǰ��Ļ�߿��
    public static float ScreenRate = Screen.height / (float)Screen.width;

    //���������rect�ߵı���
    public static float cameraRectHeightRate = DevelopHeigh / ((DevelopWidth / Screen.width) * Screen.height);
    //���������rect��ı���
    public static float cameraRectWidthRate = DevelopWidth / ((DevelopHeigh / Screen.height) * Screen.width);

    public static void CameraAdaptation(Camera camera)
    {
        //������Ļ��ʵ����Ļ����<=���������� ���º�  ��֮���Һ�
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
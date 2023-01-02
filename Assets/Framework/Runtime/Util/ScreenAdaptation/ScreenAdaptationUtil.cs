using UnityEngine;

public static class ScreenAdaptationUtil
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

    public static void FitCamera(Camera camera)
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

    //����uicanvas�е�matchWidthOrHeight
    public static void UIFitCamera(Camera camera)
    {
        //������Ļ��ʵ����Ļ����<=���������ģ����䣬������Ӧ����֮����
        if (DevelopRate <= ScreenRate)
        {
            camera.rect = new Rect(0, (1 - cameraRectHeightRate) / 2, 1, cameraRectHeightRate);
        }
        else
        {
            camera.rect = new Rect((1 - cameraRectWidthRate) / 2, 0, cameraRectWidthRate, 1);
        }
    }
}
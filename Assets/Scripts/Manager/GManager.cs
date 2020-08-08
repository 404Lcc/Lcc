using UnityEngine;

namespace Model
{
    public class GManager : MonoBehaviour
    {
        void Awake()
        {
            InitManagers();
        }
        void Start()
        {
            //开屏界面-资源更新界面-初始化IL-开始界面
            IO.panelManager.OpenPanel(PanelType.Launch);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IO.panelManager.IsOpenPanel(PanelType.Set))
                {
                    return;
                }
                if (IO.panelManager.IsOpenPanel(PanelType.Quit))
                {
                    return;
                }
                if (!IO.panelManager.IsOpenPanel(PanelType.Launch) && !IO.panelManager.IsOpenPanel(PanelType.Load))
                {
                }
            }
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    ScreenCapture.CaptureScreenshot(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + "Screenshot.png");
                }
            }
        }
        /// <summary>
        /// 初始化管理类
        /// </summary>
        private void InitManagers()
        {
            GameUtil.AddComponent<ILRuntimeManager>(gameObject);
            GameUtil.AddComponent<MonoManager>(gameObject);
            GameUtil.AddComponent<AssetManager>(gameObject);
            GameUtil.AddComponent<PanelManager>(gameObject);
            GameUtil.AddComponent<LogManager>(gameObject);
            GameUtil.AddComponent<ContainerManager>(gameObject);
            GameUtil.AddComponent<TipsManager>(gameObject);
            GameUtil.AddComponent<TipsWindowManager>(gameObject);
            GameUtil.AddComponent<LoadSceneManager>(gameObject);
        }
    }
}
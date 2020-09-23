using System;
using UnityEngine;

namespace Hotfix
{
    public class Manager : Singleton<Manager>
    {
        public override void Start()
        {
            Model.LoadSceneManager.Instance.LoadScene(SceneName.Login, () =>
            {
                PanelManager.Instance.OpenPanel(PanelType.Login);
            }, AssetType.Scene);
        }
        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (PanelManager.Instance.IsOpenPanel(PanelType.Set))
                {
                    return;
                }
                if (PanelManager.Instance.IsOpenPanel(PanelType.Quit))
                {
                    return;
                }
                if (!PanelManager.Instance.IsOpenPanel(PanelType.Load))
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
        public void InitManager()
        {
            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, true, AssetType.UI));
        }
        /// <summary>
        /// 初始化设置
        /// </summary>
        public void InitUserSet()
        {
            GameDataManager.Instance.GetUserSetData();
            AudioManager.Instance.SetVolume(UserSetData.audio, Model.Objects.audioSource);
            string name = Enum.GetName(typeof(ResolutionType), UserSetData.resolutionType).Substring(10);
            int width = int.Parse(name.Substring(0, name.IndexOf('x')));
            int height = int.Parse(name.Substring(name.IndexOf('x') + 1));
            if (UserSetData.displayModeType == DisplayModeType.FullScreen)
            {
                GameUtil.SetResolution(true, width, height);
            }
            else if (UserSetData.displayModeType == DisplayModeType.Window)
            {
                GameUtil.SetResolution(false, width, height);
            }
            else if (UserSetData.displayModeType == DisplayModeType.BorderlessWindow)
            {
                GameUtil.SetResolution(false, width, height);
                StartCoroutine(Model.DisplayMode.SetNoFrame(width, height));
            }
            QualitySettings.SetQualityLevel(6, true);
        }
    }
}
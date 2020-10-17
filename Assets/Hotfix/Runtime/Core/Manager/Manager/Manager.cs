using System;
using UnityEngine;

namespace Hotfix
{
    public class Manager : Singleton<Manager>
    {
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
                    ScreenCapture.CaptureScreenshot(PathUtil.GetPath(PathType.PersistentDataPath, "Res") + "Screenshot.png");
                }
            }
        }
        /// <summary>
        /// 初始化管理类
        /// </summary>
        public void InitManager()
        {
            Model.SceneLoadManager.Instance.LoadScene(SceneName.Login, () =>
            {
                UIEventManager.Instance.Publish(UIEventType.Login);
            }, AssetType.Scene);
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
                LccUtil.SetResolution(true, width, height);
            }
            else if (UserSetData.displayModeType == DisplayModeType.Window)
            {
                LccUtil.SetResolution(false, width, height);
            }
            else if (UserSetData.displayModeType == DisplayModeType.BorderlessWindow)
            {
                LccUtil.SetResolution(false, width, height);
                StartCoroutine(Model.DisplayMode.SetNoFrame(width, height));
            }
            QualitySettings.SetQualityLevel(6, true);
        }
    }
}
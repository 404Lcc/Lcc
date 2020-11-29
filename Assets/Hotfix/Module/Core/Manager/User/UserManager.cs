using LccModel;
using System;
using UnityEngine;

namespace LccHotfix
{
    public class UserManager : Singleton<UserManager>
    {
        public void InitManager()
        {
            GameDataManager.Instance.GetUserSetData();
            AudioManager.Instance.SetVolume(UserSetData.audio, Objects.AudioSource);
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
                StartCoroutine(DisplayMode.SetNoFrame(width, height));
            }
            QualitySettings.SetQualityLevel(6, true);
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
                    ScreenCapture.CaptureScreenshot(PathUtil.GetPath(PathType.PersistentDataPath, "Res") + "Screenshot.png");
                }
            }
        }
    }
}
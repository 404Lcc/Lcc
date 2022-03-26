using System;
using UnityEngine;

namespace LccModel
{
    public class GameSettingManager : Singleton<GameSettingManager>, IUpdate
    {
        public void InitManager()
        {
            AudioManager.Instance.SetVolume(GameDataManager.Instance.GetUserSetData().audio, Objects.AudioSource);
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                string name = Enum.GetName(typeof(ResolutionType), GameDataManager.Instance.GetUserSetData().resolutionType).Substring(10);
                int width = int.Parse(name.Substring(0, name.IndexOf('x')));
                int height = int.Parse(name.Substring(name.IndexOf('x') + 1));
                if (GameDataManager.Instance.GetUserSetData().displayModeType == DisplayModeType.FullScreen)
                {
                    LccUtil.SetResolution(true, width, height);
                }
                else if (GameDataManager.Instance.GetUserSetData().displayModeType == DisplayModeType.Window)
                {
                    LccUtil.SetResolution(false, width, height);
                }
                else if (GameDataManager.Instance.GetUserSetData().displayModeType == DisplayModeType.BorderlessWindow)
                {
                    LccUtil.SetResolution(false, width, height);
                    StartCoroutine(DisplayMode.SetNoFrame(width, height));
                }
                QualitySettings.SetQualityLevel(6, true);
            }
        }
        public override void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.C))
            {
                ScreenCapture.CaptureScreenshot($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/Screenshot.png");
            }
#endif
        }
    }
}
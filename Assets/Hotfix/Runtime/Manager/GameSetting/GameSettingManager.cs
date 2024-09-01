using LccModel;
using System;
using UnityEngine;

namespace LccHotfix
{
    public class GameSettingManager : AObjectBase
    {
        public void SetGame()
        {
            AudioManager.Instance.SetVolume(ArchiveManager.Instance.GetUserSetData().audio, GlobalManager.Instance.AudioSource);
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                string name = Enum.GetName(typeof(ResolutionType), ArchiveManager.Instance.GetUserSetData().resolutionType).Substring(10);
                int width = int.Parse(name.Substring(0, name.IndexOf('x')));
                int height = int.Parse(name.Substring(name.IndexOf('x') + 1));
                if (ArchiveManager.Instance.GetUserSetData().displayModeType == DisplayModeType.FullScreen)
                {
                    ResolutionUtility.SetResolution(true, width, height);
                }
                else if (ArchiveManager.Instance.GetUserSetData().displayModeType == DisplayModeType.Window)
                {
                    ResolutionUtility.SetResolution(false, width, height);
                }
                else if (ArchiveManager.Instance.GetUserSetData().displayModeType == DisplayModeType.BorderlessWindow)
                {
                    ResolutionUtility.SetResolution(false, width, height);
                    StartCoroutine(DisplayMode.SetNoFrame(width, height));
                }
                QualitySettings.SetQualityLevel(6, true);
            }
        }
//        public void Update()
//        {
//#if UNITY_EDITOR
//            if (Input.GetKeyDown(KeyCode.C))
//            {
//                ScreenCapture.CaptureScreenshot($"{PathUtil.GetPersistentDataPath("Screenshot")}{AssetSuffix.Png}");
//            }
//#endif
//        }
    }
}
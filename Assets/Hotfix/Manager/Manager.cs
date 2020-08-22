using System;
using UnityEngine;

namespace Hotfix
{
    public class Manager : MonoBehaviour
    {
        void Awake()
        {
            InitManagers();
        }
        void Start()
        {
            Model.IO.panelManager.ClearPanel(Model.PanelType.Launch);
            IO.panelManager.OpenPanel(PanelType.Load);
            Model.IO.loadSceneManager.LoadScene(SceneName.Login, () =>
            {
                IO.panelManager.OpenPanel(PanelType.Login);
            }, AssetType.Scene);
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
                if (!IO.panelManager.IsOpenPanel(PanelType.Load))
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
            GameUtil.AddComponent<PanelManager>(gameObject);
            GameUtil.AddComponent<LogManager>(gameObject);
            GameUtil.AddComponent<LanguageManager>(gameObject);
            GameUtil.AddComponent<GameDataManager>(gameObject);
            GameUtil.AddComponent<GameEventManager>(gameObject);
            GameUtil.AddComponent<AudioManager>(gameObject);
            GameUtil.AddComponent<VoiceManager>(gameObject);
            GameUtil.AddComponent<VideoManager>(gameObject);
            GameUtil.AddComponent<CommandManager>(gameObject);
            GameUtil.AddComponent<TimerManager>(gameObject);
            GameUtil.AddComponent<GameTimeManager>(gameObject);
            GameUtil.AddComponent<CharacterManager>(gameObject);
        }
        /// <summary>
        /// 初始化设置
        /// </summary>
        private void InitGameSet()
        {
            IO.gameDataManager.GetUserSetData();
            IO.audioManager.SetVolume(UserSetData.audio, Model.IO.audioSource);
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
                StartCoroutine(DisplayMode.SetNoFrame(width, height));
            }
            QualitySettings.SetQualityLevel(6, true);
        }
    }
}
using UnityEngine;

namespace Hotfix
{
    public class IO : MonoBehaviour
    {
        private static GManager _gManager;
        private static PanelManager _panelManager;
        private static LogManager _logManager;
        private static LanguageManager _languageManager;
        private static GameDataManager _gamedataManager;
        private static GameEventManager _gameeventManager;
        private static AudioManager _audioManager;
        private static VoiceManager _voiceManager;
        private static VideoManager _videoManager;
        private static CommandManager _commandManager;
        private static TimerManager _timerManager;
        private static GameTimeManager _gametimeManager;
        private static CharacterManager _characterManager;
        private static AStarManager _astarManager;
        public static GManager gManager
        {
            get
            {
                if (IO._gManager == null)
                {
                    GameObject manager = GameUtil.GetGameObjectConvertedToTag("HotfixManager");
                    if (manager == null) return null;
                    IO._gManager = GameUtil.GetComponent<GManager>(manager);
                }
                return IO._gManager;
            }
        }
        public static PanelManager panelManager
        {
            get
            {
                if (IO._panelManager == null)
                {
                    IO._panelManager = GameUtil.GetComponent<PanelManager>(IO.gManager.gameObject);
                }
                return IO._panelManager;
            }
        }
        public static LogManager logManager
        {
            get
            {
                if (IO._logManager == null)
                {
                    IO._logManager = GameUtil.GetComponent<LogManager>(IO.gManager.gameObject);
                }
                return IO._logManager;
            }
        }
        public static LanguageManager languageManager
        {
            get
            {
                if (IO._languageManager == null)
                {
                    IO._languageManager = GameUtil.GetComponent<LanguageManager>(IO.gManager.gameObject);
                }
                return IO._languageManager;
            }
        }
        public static GameDataManager gamedataManager
        {
            get
            {
                if (IO._gamedataManager == null)
                {
                    IO._gamedataManager = GameUtil.GetComponent<GameDataManager>(IO.gManager.gameObject);
                }
                return IO._gamedataManager;
            }
        }
        public static GameEventManager gameeventManager
        {
            get
            {
                if (IO._gameeventManager == null)
                {
                    IO._gameeventManager = GameUtil.GetComponent<GameEventManager>(IO.gManager.gameObject);
                }
                return IO._gameeventManager;
            }
        }
        public static AudioManager audioManager
        {
            get
            {
                if (IO._audioManager == null)
                {
                    IO._audioManager = GameUtil.GetComponent<AudioManager>(IO.gManager.gameObject);
                }
                return IO._audioManager;
            }
        }
        public static VoiceManager voiceManager
        {
            get
            {
                if (IO._voiceManager == null)
                {
                    IO._voiceManager = GameUtil.GetComponent<VoiceManager>(IO.gManager.gameObject);
                }
                return IO._voiceManager;
            }
        }
        public static VideoManager videoManager
        {
            get
            {
                if (IO._videoManager == null)
                {
                    IO._videoManager = GameUtil.GetComponent<VideoManager>(IO.gManager.gameObject);
                }
                return IO._videoManager;
            }
        }
        public static CommandManager commandManager
        {
            get
            {
                if (IO._commandManager == null)
                {
                    IO._commandManager = GameUtil.GetComponent<CommandManager>(IO.gManager.gameObject);
                }
                return IO._commandManager;
            }
        }
        public static TimerManager timerManager
        {
            get
            {
                if (IO._timerManager == null)
                {
                    IO._timerManager = GameUtil.GetComponent<TimerManager>(IO.gManager.gameObject);
                }
                return IO._timerManager;
            }
        }
        public static GameTimeManager gametimeManager
        {
            get
            {
                if (IO._gametimeManager == null)
                {
                    IO._gametimeManager = GameUtil.GetComponent<GameTimeManager>(IO.gManager.gameObject);
                }
                return IO._gametimeManager;
            }
        }
        public static CharacterManager characterManager
        {
            get
            {
                if (IO._characterManager == null)
                {
                    IO._characterManager = GameUtil.GetComponent<CharacterManager>(IO.gManager.gameObject);
                }
                return IO._characterManager;
            }
        }
        public static AStarManager astarManager
        {
            get
            {
                if (IO._astarManager == null)
                {
                    IO._astarManager = GameUtil.GetComponent<AStarManager>(IO.gManager.gameObject);
                }
                return IO._astarManager;
            }
        }
    }
}
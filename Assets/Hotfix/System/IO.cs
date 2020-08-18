using UnityEngine;

namespace Hotfix
{
    public class IO : MonoBehaviour
    {
        private static Manager _manager;
        private static PanelManager _panelManager;
        private static LogManager _logManager;
        private static LanguageManager _languageManager;
        private static GameDataManager _gameDataManager;
        private static GameEventManager _gameEventManager;
        private static AudioManager _audioManager;
        private static VoiceManager _voiceManager;
        private static VideoManager _videoManager;
        private static CommandManager _commandManager;
        private static TimerManager _timerManager;
        private static GameTimeManager _gameTimeManager;
        private static CharacterManager _characterManager;
        public static Manager manager
        {
            get
            {
                if (_manager == null)
                {
                    GameObject manager = GameUtil.GetGameObjectConvertedToTag("HotfixManager");
                    if (manager == null) return null;
                    _manager = GameUtil.GetComponent<Manager>(manager);
                }
                return _manager;
            }
        }
        public static PanelManager panelManager
        {
            get
            {
                if (_panelManager == null)
                {
                    _panelManager = GameUtil.GetComponent<PanelManager>(manager.gameObject);
                }
                return _panelManager;
            }
        }
        public static LogManager logManager
        {
            get
            {
                if (_logManager == null)
                {
                    _logManager = GameUtil.GetComponent<LogManager>(manager.gameObject);
                }
                return _logManager;
            }
        }
        public static LanguageManager languageManager
        {
            get
            {
                if (_languageManager == null)
                {
                    _languageManager = GameUtil.GetComponent<LanguageManager>(manager.gameObject);
                }
                return _languageManager;
            }
        }
        public static GameDataManager gameDataManager
        {
            get
            {
                if (_gameDataManager == null)
                {
                    _gameDataManager = GameUtil.GetComponent<GameDataManager>(manager.gameObject);
                }
                return _gameDataManager;
            }
        }
        public static GameEventManager gameEventManager
        {
            get
            {
                if (_gameEventManager == null)
                {
                    _gameEventManager = GameUtil.GetComponent<GameEventManager>(manager.gameObject);
                }
                return _gameEventManager;
            }
        }
        public static AudioManager audioManager
        {
            get
            {
                if (_audioManager == null)
                {
                    _audioManager = GameUtil.GetComponent<AudioManager>(manager.gameObject);
                }
                return _audioManager;
            }
        }
        public static VoiceManager voiceManager
        {
            get
            {
                if (_voiceManager == null)
                {
                    _voiceManager = GameUtil.GetComponent<VoiceManager>(manager.gameObject);
                }
                return _voiceManager;
            }
        }
        public static VideoManager videoManager
        {
            get
            {
                if (_videoManager == null)
                {
                    _videoManager = GameUtil.GetComponent<VideoManager>(manager.gameObject);
                }
                return _videoManager;
            }
        }
        public static CommandManager commandManager
        {
            get
            {
                if (_commandManager == null)
                {
                    _commandManager = GameUtil.GetComponent<CommandManager>(manager.gameObject);
                }
                return _commandManager;
            }
        }
        public static TimerManager timerManager
        {
            get
            {
                if (_timerManager == null)
                {
                    _timerManager = GameUtil.GetComponent<TimerManager>(manager.gameObject);
                }
                return _timerManager;
            }
        }
        public static GameTimeManager gameTimeManager
        {
            get
            {
                if (_gameTimeManager == null)
                {
                    _gameTimeManager = GameUtil.GetComponent<GameTimeManager>(manager.gameObject);
                }
                return _gameTimeManager;
            }
        }
        public static CharacterManager characterManager
        {
            get
            {
                if (_characterManager == null)
                {
                    _characterManager = GameUtil.GetComponent<CharacterManager>(manager.gameObject);
                }
                return _characterManager;
            }
        }
    }
}
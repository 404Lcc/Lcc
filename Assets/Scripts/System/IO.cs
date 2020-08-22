using UnityEngine;
using UnityEngine.Video;

namespace Model
{
    public class IO : MonoBehaviour
    {
        private static GameObject _gui;
        private static Manager _manager;
        private static AudioSource _audioSource;
        private static VideoPlayer _videoPlayer;
        private static ILRuntimeManager _ilRuntimeManager;
        private static MonoManager _monoManager;
        private static AssetManager _assetManager;
        private static PanelManager _panelManager;
        private static LogManager _logManager;
        private static ContainerManager _containerManager;
        private static TipsManager _tipsManager;
        private static TipsWindowManager _tipsWindowManager;
        private static LoadSceneManager _loadSceneManager;
        private static AStarManager _aStarManager;
        public static GameObject gui
        {
            get
            {
                if (_gui == null)
                {
                    _gui = GameUtil.GetGameObjectConvertedToTag("GUI");
                }
                return _gui;
            }
        }
        public static Manager manager
        {
            get
            {
                if (_manager == null)
                {
                    GameObject manager = GameUtil.GetGameObjectConvertedToTag("ModelManager");
                    if (manager == null) return null;
                    _manager = GameUtil.GetComponent<Manager>(manager);
                }
                return _manager;
            }
        }
        public static AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    GameObject audiosource = GameUtil.GetGameObjectConvertedToTag("AudioSource");
                    if (audiosource == null) return null;
                    _audioSource = GameUtil.GetComponent<AudioSource>(audiosource);
                }
                return _audioSource;
            }
        }
        public static VideoPlayer videoPlayer
        {
            get
            {
                if (_videoPlayer == null)
                {
                    GameObject videoplayer = GameUtil.GetGameObjectConvertedToTag("AudioSource");
                    if (videoplayer == null) return null;
                    _videoPlayer = GameUtil.GetComponent<VideoPlayer>(videoplayer);
                }
                return _videoPlayer;
            }
        }
        public static ILRuntimeManager ilRuntimeManager
        {
            get
            {
                if (_ilRuntimeManager == null)
                {
                    _ilRuntimeManager = GameUtil.GetComponent<ILRuntimeManager>(manager.gameObject);
                }
                return _ilRuntimeManager;
            }
        }
        public static MonoManager monoManager
        {
            get
            {
                if (_monoManager == null)
                {
                    _monoManager = GameUtil.GetComponent<MonoManager>(manager.gameObject);
                }
                return _monoManager;
            }
        }
        public static AssetManager assetManager
        {
            get
            {
                if (_assetManager == null)
                {
                    _assetManager = GameUtil.GetComponent<AssetManager>(manager.gameObject);
                }
                return _assetManager;
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
        public static ContainerManager containerManager
        {
            get
            {
                if (_containerManager == null)
                {
                    _containerManager = GameUtil.GetComponent<ContainerManager>(manager.gameObject);
                }
                return _containerManager;
            }
        }
        public static TipsManager tipsManager
        {
            get
            {
                if (_tipsManager == null)
                {
                    _tipsManager = GameUtil.GetComponent<TipsManager>(manager.gameObject);
                }
                return _tipsManager;
            }
        }
        public static TipsWindowManager tipsWindowManager
        {
            get
            {
                if (_tipsWindowManager == null)
                {
                    _tipsWindowManager = GameUtil.GetComponent<TipsWindowManager>(manager.gameObject);
                }
                return _tipsWindowManager;
            }
        }
        public static LoadSceneManager loadSceneManager
        {
            get
            {
                if (_loadSceneManager == null)
                {
                    _loadSceneManager = GameUtil.GetComponent<LoadSceneManager>(manager.gameObject);
                }
                return _loadSceneManager;
            }
        }
        public static AStarManager aStarManager
        {
            get
            {
                if (_aStarManager == null)
                {
                    _aStarManager = GameUtil.GetComponent<AStarManager>(manager.gameObject);
                }
                return _aStarManager;
            }
        }
    }
}
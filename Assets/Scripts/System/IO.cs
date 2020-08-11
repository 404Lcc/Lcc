using UnityEngine;
using UnityEngine.Video;

namespace Model
{
    public class IO : MonoBehaviour
    {
        private static GameObject _gui;
        private static GManager _gManager;
        private static AudioSource _audioSource;
        private static VideoPlayer _videoplayer;
        private static ILRuntimeManager _ilruntimeManager;
        private static MonoManager _monoManager;
        private static AssetManager _assetManager;
        private static PanelManager _panelManager;
        private static LogManager _logManager;
        private static ContainerManager _containerManager;
        private static TipsManager _tipsManager;
        private static TipsWindowManager _tipswindowManager;
        private static LoadSceneManager _loadsceneManager;
        private static AStarManager _astarManager;
        public static GameObject gui
        {
            get
            {
                if (IO._gui == null)
                {
                    IO._gui = GameUtil.GetGameObjectConvertedToTag("GUI");
                }
                return IO._gui;
            }
        }
        public static GManager gManager
        {
            get
            {
                if (IO._gManager == null)
                {
                    GameObject manager = GameUtil.GetGameObjectConvertedToTag("ModelManager");
                    if (manager == null) return null;
                    IO._gManager = GameUtil.GetComponent<GManager>(manager);
                }
                return IO._gManager;
            }
        }
        public static AudioSource audioSource
        {
            get
            {
                if (IO._audioSource == null)
                {
                    GameObject audiosource = GameUtil.GetGameObjectConvertedToTag("AudioSource");
                    if (audiosource == null) return null;
                    IO._audioSource = GameUtil.GetComponent<AudioSource>(audiosource);
                }
                return IO._audioSource;
            }
        }
        public static VideoPlayer videoPlayer
        {
            get
            {
                if (IO._videoplayer == null)
                {
                    GameObject videoplayer = GameUtil.GetGameObjectConvertedToTag("AudioSource");
                    if (videoplayer == null) return null;
                    IO._videoplayer = GameUtil.GetComponent<VideoPlayer>(videoplayer);
                }
                return IO._videoplayer;
            }
        }
        public static ILRuntimeManager ilruntimeManager
        {
            get
            {
                if (IO._ilruntimeManager == null)
                {
                    IO._ilruntimeManager = GameUtil.GetComponent<ILRuntimeManager>(IO.gManager.gameObject);
                }
                return IO._ilruntimeManager;
            }
        }
        public static MonoManager monoManager
        {
            get
            {
                if (IO._monoManager == null)
                {
                    IO._monoManager = GameUtil.GetComponent<MonoManager>(IO.gManager.gameObject);
                }
                return IO._monoManager;
            }
        }
        public static AssetManager assetManager
        {
            get
            {
                if (IO._assetManager == null)
                {
                    IO._assetManager = GameUtil.GetComponent<AssetManager>(IO.gManager.gameObject);
                }
                return IO._assetManager;
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
        public static ContainerManager containerManager
        {
            get
            {
                if (IO._containerManager == null)
                {
                    IO._containerManager = GameUtil.GetComponent<ContainerManager>(IO.gManager.gameObject);
                }
                return IO._containerManager;
            }
        }
        public static TipsManager tipsManager
        {
            get
            {
                if (IO._tipsManager == null)
                {
                    IO._tipsManager = GameUtil.GetComponent<TipsManager>(IO.gManager.gameObject);
                }
                return IO._tipsManager;
            }
        }
        public static TipsWindowManager tipswindowManager
        {
            get
            {
                if (IO._tipswindowManager == null)
                {
                    IO._tipswindowManager = GameUtil.GetComponent<TipsWindowManager>(IO.gManager.gameObject);
                }
                return IO._tipswindowManager;
            }
        }
        public static LoadSceneManager loadsceneManager
        {
            get
            {
                if (IO._loadsceneManager == null)
                {
                    IO._loadsceneManager = GameUtil.GetComponent<LoadSceneManager>(IO.gManager.gameObject);
                }
                return IO._loadsceneManager;
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
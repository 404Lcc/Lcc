using UnityEngine;

namespace Model
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            InitGUI();
            InitAudioSource();
            InitVideoPlayer();
            Manager.Instance.InitManagers();
        }
        /// <summary>
        /// 初始化UI
        /// </summary>
        public void InitGUI()
        {
            if (Objects.gui == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/Canvas")) as GameObject;
                original.name = "Canvas";
                DontDestroyOnLoad(original);
            }
        }
        /// <summary>
        /// 初始化AudioSource
        /// </summary>
        public void InitAudioSource()
        {
            if (Objects.audioSource == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/AudioSource")) as GameObject;
                original.name = "AudioSource";
                DontDestroyOnLoad(original);
            }
        }
        /// <summary>
        /// 初始化VideoPlayer
        /// </summary>
        public void InitVideoPlayer()
        {
            if (Objects.videoPlayer == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/VideoPlayer")) as GameObject;
                original.name = "VideoPlayer";
                DontDestroyOnLoad(original);
            }
        }
    }
}
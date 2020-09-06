using UnityEngine;

namespace Model
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            InitGUI();
            InitManager();
            InitAudioSource();
            InitVideoPlayer();
        }
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitGUI()
        {
            if (Objects.gui == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/Canvas")) as GameObject;
                original.name = "Canvas";
                DontDestroyOnLoad(original);
            }
        }
        /// <summary>
        /// 初始化管理器
        /// </summary>
        private void InitManager()
        {
            if (Objects.manager == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/ModelManager")) as GameObject;
                original.name = "ModelManager";
                DontDestroyOnLoad(original);
            }
        }
        /// <summary>
        /// 初始化AudioSource
        /// </summary>
        private void InitAudioSource()
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
        private void InitVideoPlayer()
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
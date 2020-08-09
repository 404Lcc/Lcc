using UnityEngine;

namespace Model
{
    public class GlobalGenerator : MonoBehaviour
    {
        void Start()
        {
            InitGui();
            InitGManager();
            InitAudioSource();
            InitVideoPlayer();
        }
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitGui()
        {
            if (IO.gui == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/Canvas")) as GameObject;
                original.name = "Canvas";
                DontDestroyOnLoad(original);
            }
        }
        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        private void InitGManager()
        {
            if (IO.gManager == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/GManager")) as GameObject;
                original.name = "GManager";
                DontDestroyOnLoad(original);
            }
        }
        /// <summary>
        /// 初始化AudioSource
        /// </summary>
        private void InitAudioSource()
        {
            if (IO.audioSource == null)
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
            if (IO.videoPlayer == null)
            {
                GameObject original = Instantiate(Resources.Load("Game/VideoPlayer")) as GameObject;
                original.name = "VideoPlayer";
                DontDestroyOnLoad(original);
            }
        }
    }
}
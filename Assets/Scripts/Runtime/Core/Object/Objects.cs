using UnityEngine;
using UnityEngine.Video;

namespace Model
{
    public class Objects
    {
        private static GameObject _gui;
        private static AudioSource _audioSource;
        private static VideoPlayer _videoPlayer;
        public static GameObject gui
        {
            get
            {
                if (_gui == null)
                {
                    _gui = "GUI".GetGameObjectToTag();
                }
                return _gui;
            }
        }
        public static AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    GameObject gameObject = "AudioSource".GetGameObjectToTag();
                    if (gameObject == null) return null;
                    _audioSource = gameObject.GetComponent<AudioSource>();
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
                    GameObject gameObject = "AudioSource".GetGameObjectToTag();
                    if (gameObject == null) return null;
                    _videoPlayer = gameObject.GetComponent<VideoPlayer>();
                }
                return _videoPlayer;
            }
        }
    }
}
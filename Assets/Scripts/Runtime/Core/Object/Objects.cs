using UnityEngine;
using UnityEngine.Video;

namespace LccModel
{
    public class Objects
    {
        private static Canvas _canvas;
        private static AudioSource _audioSource;
        private static VideoPlayer _videoPlayer;
        public static Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                {
                    GameObject gameObject = "Canvas".GetGameObjectToTag();
                    if (gameObject == null) return null;
                    _canvas = gameObject.GetComponent<Canvas>();
                }
                return _canvas;
            }
        }
        public static AudioSource AudioSource
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
        public static VideoPlayer VideoPlayer
        {
            get
            {
                if (_videoPlayer == null)
                {
                    GameObject gameObject = "VideoPlayer".GetGameObjectToTag();
                    if (gameObject == null) return null;
                    _videoPlayer = gameObject.GetComponent<VideoPlayer>();
                }
                return _videoPlayer;
            }
        }
    }
}
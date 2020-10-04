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
                    _gui = Util.GetGameObjectConvertedToTag("GUI");
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
                    GameObject audiosource = Util.GetGameObjectConvertedToTag("AudioSource");
                    if (audiosource == null) return null;
                    _audioSource = Util.GetComponent<AudioSource>(audiosource);
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
                    GameObject videoplayer = Util.GetGameObjectConvertedToTag("AudioSource");
                    if (videoplayer == null) return null;
                    _videoPlayer = Util.GetComponent<VideoPlayer>(videoplayer);
                }
                return _videoPlayer;
            }
        }
    }
}
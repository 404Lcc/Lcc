using UnityEngine;
using UnityEngine.Video;

namespace LccHotfix
{
    internal class GlobalManager : Module
    {
        public static GlobalManager Instance => Entry.GetModule<GlobalManager>();
        public Transform Global { get; set; }

        public Camera MainCamera { get; set; }



        public AudioSource AudioSource { get; set; }
        public VideoPlayer VideoPlayer { get; set; }
        public GlobalManager()
        {

            Global = GameObject.Find("Global").transform;

            MainCamera = GameObject.Find("Global/MainCamera").GetComponent<Camera>();

            AudioSource = GameObject.Find("Global/AudioSource").GetComponent<AudioSource>();
            VideoPlayer = GameObject.Find("Global/VideoPlayer").GetComponent<VideoPlayer>();
        }
        internal override void Shutdown()
        {
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            Global = null;
            MainCamera = null;
            AudioSource = null;
            VideoPlayer = null;
        }
    }
}
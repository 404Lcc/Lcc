using UnityEngine;
using UnityEngine.Video;

namespace LccHotfix
{
    internal class GlobalManager : Module
    {
        public static GlobalManager Instance => Entry.GetModule<GlobalManager>();
        public Transform Global { get; set; }




        public AudioSource Music { get; set; }
        public AudioSource SoundFX { get; set; }
        public VideoPlayer VideoPlayer { get; set; }
        public GlobalManager()
        {

            Global = GameObject.Find("Global").transform;


            Music = GameObject.Find("Global/Music").GetComponent<AudioSource>();
            SoundFX = GameObject.Find("Global/SoundFX").GetComponent<AudioSource>();
            VideoPlayer = GameObject.Find("Global/VideoPlayer").GetComponent<VideoPlayer>();
        }
        internal override void Shutdown()
        {
            Global = null;
            Music = null;
            SoundFX = null;
            VideoPlayer = null;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }
    }
}
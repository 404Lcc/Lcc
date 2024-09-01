using UnityEngine;
using UnityEngine.Video;

namespace LccHotfix
{
    public class GlobalManager : AObjectBase
    {
        public static GlobalManager Instance { get; set; }
        public Transform Global { get; set; }

        public Camera MainCamera { get; set; }
        public Camera UICamera { get; set; }


        public Transform UIRoot { get; set; }
        public Transform NormalRoot { get; set; }//0
        public Transform FixedRoot { get; set; }//10
        public Transform PopupRoot { get; set; }//20
        public Transform RemoveRoot { get; set; }//10



        public Transform UnitRoot { get; set; }
        public Transform PoolRoot { get; set; }



        public AudioSource AudioSource { get; set; }
        public VideoPlayer VideoPlayer { get; set; }
        public override void Awake()
        {
            base.Awake();

            Instance = this;

            Global = GameObject.Find("Global").transform;

            MainCamera = GameObject.Find("Global/MainCamera").GetComponent<Camera>();
            UICamera = GameObject.Find("Global/UICamera").GetComponent<Camera>();

            UIRoot = GameObject.Find("Global/UIRoot").transform;
            NormalRoot = GameObject.Find("Global/UIRoot/NormalRoot").transform;
            FixedRoot = GameObject.Find("Global/UIRoot/FixedRoot").transform;
            PopupRoot = GameObject.Find("Global/UIRoot/PopUpRoot").transform;
            RemoveRoot = GameObject.Find("Global/UIRoot/RemoveRoot").transform;


            UnitRoot = GameObject.Find("Global/UnitRoot").transform;
            PoolRoot = GameObject.Find("Global/PoolRoot").transform;

            AudioSource = GameObject.Find("Global/AudioSource").GetComponent<AudioSource>();
            VideoPlayer = GameObject.Find("Global/VideoPlayer").GetComponent<VideoPlayer>();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;

            Global = null;

            UIRoot = null;
            NormalRoot = null;
            FixedRoot = null;
            PopupRoot = null;


            UnitRoot = null;
            PoolRoot = null;

            AudioSource = null;
            VideoPlayer = null;
        }
    }
}
using UnityEngine;
using UnityEngine.Video;

namespace LccModel
{
    public class GlobalManager : Singleton<GlobalManager>
    {
        public Transform Global { get; set; }




        public Transform UIRoot { get; set; }
        public Transform NormalRoot { get; set; }//0
        public Transform FixedRoot { get; set; }//10
        public Transform PopupRoot { get; set; }//20



        public Transform UnitRoot { get; set; }
        public Transform PoolRoot { get; set; }



        public AudioSource AudioSource { get; set; }
        public VideoPlayer VideoPlayer { get; set; }


        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            Global = GameObject.Find("Global").transform;
   


            UIRoot = GameObject.Find("Global/UIRoot").transform;
            NormalRoot = GameObject.Find("Global/UIRoot/NormalRoot").transform;
            FixedRoot = GameObject.Find("Global/UIRoot/FixedRoot").transform;
            PopupRoot = GameObject.Find("Global/UIRoot/PopUpRoot").transform;



            UnitRoot = GameObject.Find("Global/UnitRoot").transform;
            PoolRoot = GameObject.Find("Global/PoolRoot").transform;

            AudioSource = GameObject.Find("Global/AudioSource").GetComponent<AudioSource>();
            VideoPlayer = GameObject.Find("Global/VideoPlayer").GetComponent<VideoPlayer>();
        }
    }
}
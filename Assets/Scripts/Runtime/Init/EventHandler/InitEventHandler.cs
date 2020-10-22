using UnityEngine;

namespace LccModel
{
    public class InitEventHandler : AEvent<Start>
    {
        public override void Publish(Start data)
        {
            Object.DontDestroyOnLoad(AssetManager.Instance.LoadGameObject("Canvas", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(AssetManager.Instance.LoadGameObject("AudioSource", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(AssetManager.Instance.LoadGameObject("VideoPlayer", false, false, AssetType.Game));
            Manager.Instance.InitManager();
        }
    }
}
using ET;
using UnityEngine;

namespace LccModel
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {
            Object.DontDestroyOnLoad(Instantiate("Game/Canvas"));
            Object.DontDestroyOnLoad(Instantiate("Game/AudioSource"));
            Object.DontDestroyOnLoad(Instantiate("Game/VideoPlayer"));

            DownloadManager.Instance.InitManager();
            await AssetBundleManager.Instance.InitManager();

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(AssetType.Prefab, AssetType.Panel));
            TipsManager.Instance.InitManager(new TipsPool(10));
            TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            //步骤
            //打开开屏界面
            //如果是ab模式进入检测资源更新界面
            //初始化主工程并初始化热更层
            //打开登录界面
            UIEventManager.Instance.Publish(UIEventType.Launch);
            await ETTask.CompletedTask;
        }
        public GameObject Instantiate(string path)
        {
            GameObject asset = Resources.Load<GameObject>(path);
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = path;
            return gameObject;
        }
    }
}
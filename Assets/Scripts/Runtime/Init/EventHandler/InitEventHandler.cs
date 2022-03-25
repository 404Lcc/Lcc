using ET;
using System.IO;
using System.Threading.Tasks;
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

            await LaunchPanel.Instance.ShowLaunchAnim();



            DownloadManager.Instance.InitManager();
            await AssetBundleManager.Instance.InitManager();

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(AssetType.Prefab, AssetType.Panel));
            TipsManager.Instance.InitManager(new TipsPool(10));
            TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            ConfigManager.Instance.InitManager();


            await LoadingPanel.Instance.UpdateLoadingPercent(76, 100);

#if ILRuntime
            ILRuntimeManager.Instance.InitManager();
#else
            MonoManager.Instance.InitManager();
#endif

            LoadingPanel.Instance.ClosePanel();
        }
        public GameObject Instantiate(string path)
        {
            GameObject asset = Resources.Load<GameObject>(path);
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = Path.GetFileNameWithoutExtension(path);
            return gameObject;
        }
    }
}
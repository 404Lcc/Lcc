using ET;
using LccModel;
using System;

namespace LccHotfix
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {



            ConfigManager.Instance.InitManager();

      




            await LoadingPanel.Instance.UpdateLoadingPercent(51, 60);
            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Login, AssetType.Scene);
            await LoadingPanel.Instance.UpdateLoadingPercent(61, 70);


     

            await LoadingPanel.Instance.UpdateLoadingPercent(71, 100);



            UIEventManager.Instance.Publish(UIEventType.Login);

            LoadingPanel.Instance.ClosePanel();

        }
    }
}
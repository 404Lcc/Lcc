using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Main)]
    public class MainSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();
            Debug.Log("Main" + "进入");

            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Main, AssetType.Scene);

            //忽略增加main的导航数据
            //清除已经存在的导航数据，中断操作
            PanelManager.Instance.ShowPanel(PanelType.UIMain, new ShowPanelData(false, true, null, true, false, true));

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Main" + "退出");
        }
    }
}
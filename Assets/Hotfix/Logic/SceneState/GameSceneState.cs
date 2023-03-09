using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateName.Game)]
    public class GameSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();
            Debug.Log("Game" + "进入");

            PanelManager.Instance.HidePanel(PanelType.Top);

            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Game, AssetType.Scene);

            //忽略增加main的导航数据
            //清除已经存在的导航数据，中断操作
            PanelManager.Instance.ShowPanel(PanelType.Game, new ShowPanelData(false, true, null, true, false, true));

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Game" + "退出");
        }
    }
}
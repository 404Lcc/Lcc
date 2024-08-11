using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Main)]
    public class MainSceneState : SceneState
    {
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);
            Debug.Log("Main" + "进入");

            //忽略增加main的导航数据
            //清除已经存在的导航数据，中断操作
            PanelManager.Instance.ShowPanel(PanelType.UIMain, new ShowPanelData(false, true, null, true, false, true));

        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Main" + "退出");
        }
    }
}
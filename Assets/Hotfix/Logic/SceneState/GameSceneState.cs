using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Game)]
    public class GameSceneState : SceneState
    {
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);
            Debug.Log("Game" + "进入");

            PanelManager.Instance.HidePanel(PanelType.UITop);

            var combat1 = CombatContext.Instance.AddCombat(1);
            var combat2 = CombatContext.Instance.AddCombat(2, TagType.Enemy);
            combat2.TransformComponent.position = new Vector3(0, 10, 0);
            combat2.OrcaComponent.SetAgent2DPos(new Vector3(0, 10, 0));
            combat2.AttachSkill(1);
            combat2.AddComponent<FSMComponet>();

            //忽略增加main的导航数据
            //清除已经存在的导航数据，中断操作
            PanelManager.Instance.ShowPanel(PanelType.UIGame, new ShowPanelData(false, true, null, true, false, true));





         

        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Game" + "退出");
        }
    }
}
using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Game)]
    public class GameSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();
            Debug.Log("Game" + "进入");

            PanelManager.Instance.HidePanel(PanelType.Top);

            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Game, AssetType.Scene);

            var combat1 = CombatContext.Instance.AddCombat(1);
            var combat2 = CombatContext.Instance.AddCombat(2, TagType.Enemy);
            combat2.TransformComponent.position = new Vector3(0, 10, 0);
            combat2.AttachSkill(1);
            combat2.AddComponent<AIComponent>(Vector3.zero).SetState(new IdleState());

            //忽略增加main的导航数据
            //清除已经存在的导航数据，中断操作
            PanelManager.Instance.ShowPanel(PanelType.Game, new ShowPanelData(false, true, null, true, false, true));


            //释放普攻有执行体，执行体会在下一帧执行所以需要等待下一帧使用道具
            await Timer.Instance.WaitAsync(1000);



         

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Game" + "退出");
        }
    }
}
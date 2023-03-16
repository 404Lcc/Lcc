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



            var combat1 = CombatContext.Instance.AddCombat();
            var combat2 = CombatContext.Instance.AddCombat();


            //释放普攻
            combat1.GetComponent<SpellAttackComponent>().SpellAttackWithTarget(combat2);




            var item = combat1.AttachItem(UnityEngine.Resources.Load<ItemConfigObject>("Item_1"));
            //释放普攻有执行体，执行体会在下一帧执行所以需要等待下一帧使用道具
            await Timer.Instance.WaitAsync(1000);



            //使用物品
            combat1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combat2);

        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Game" + "退出");
        }
    }
}
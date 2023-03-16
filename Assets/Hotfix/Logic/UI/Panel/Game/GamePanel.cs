using LccModel;

namespace LccHotfix
{
    public class GameModel : ViewModelBase
    {
    }
    public class GamePanel : UIPanel<GameModel>
    {
        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);

        }
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.NormalNavigation;
        }



        public override async void OnShow(Panel panel, AObjectBase contextData = null)
        {
            base.OnShow(panel, contextData);


            var combat1 = CombatContext.Instance.AddChildren<Combat>();
            var combat2 = CombatContext.Instance.AddChildren<Combat>();


            //释放普攻
            combat1.GetComponent<SpellAttackComponent>().SpellAttackWithTarget(combat2);




            var item = combat1.AttachItem(UnityEngine.Resources.Load<ItemConfigObject>("Item_1"));
            //释放普攻有执行体，执行体会在下一帧执行所以需要等待下一帧使用道具
            await Timer.Instance.WaitAsync(1000);



            //使用物品
            combat1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combat2);
        }
    }
}
namespace LccModel
{
    public class Test : AObjectBase
    {
        public CombatEntity combatEntity1;
        public CombatEntity combatEntity2;
        public override async void InitData(object[] datas)
        {
            base.InitData(datas);


            combatEntity1 = AddChildren<CombatEntity>();
            combatEntity2 = AddChildren<CombatEntity>();
            combatEntity1.GetComponent<SpellAttackComponent>().SpellAttackWithTarget(combatEntity2);




            var item = combatEntity1.AttachItem(UnityEngine.Resources.Load<ItemConfigObject>("Item_1"));
            //释放普攻有执行体，执行体会在下一帧执行所以需要等待下一帧使用道具
            await Timer.Instance.WaitAsync(1000);
            combatEntity1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combatEntity2);
        }
    }
}
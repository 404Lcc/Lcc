namespace LccModel
{
    public class Test : AObjectBase
    {
        public CombatEntity combatEntity1;
        public CombatEntity combatEntity2;
        public override void InitData(object[] datas)
        {
            base.InitData(datas);


            combatEntity1 = AddChildren<CombatEntity>();
            combatEntity2 = AddChildren<CombatEntity>();
            combatEntity1.GetComponent<SpellAttackComponent>().SpellAttackWithTarget(combatEntity2);
        }
    }
}
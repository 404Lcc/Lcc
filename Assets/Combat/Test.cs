using UnityEngine;

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

            combatEntity1.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("战斗1 收到伤害" + damageAction.damageValue);
            });
            combatEntity2.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("战斗2 收到伤害" + damageAction.damageValue);
            });

            combatEntity1.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("战斗1 收到治疗" + damageAction.cureValue);
            });
            combatEntity2.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("战斗2 收到治疗" + damageAction.cureValue);
            });
        }
    }
}
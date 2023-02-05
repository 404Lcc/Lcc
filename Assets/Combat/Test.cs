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
            //�ͷ��չ���ִ���壬ִ���������һִ֡��������Ҫ�ȴ���һ֡ʹ�õ���
            await Timer.Instance.WaitAsync(1000);
            combatEntity1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combatEntity2);

            combatEntity1.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("ս��1 �յ��˺�" + damageAction.damageValue);
            });
            combatEntity2.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("ս��2 �յ��˺�" + damageAction.damageValue);
            });

            combatEntity1.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("ս��1 �յ�����" + damageAction.cureValue);
            });
            combatEntity2.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("ս��2 �յ�����" + damageAction.cureValue);
            });
        }
    }
}
using UnityEngine;

namespace LccModel
{
    public class Test : AObjectBase
    {
        public Combat combat1;
        public Combat combat2;
        public override async void InitData(object[] datas)
        {
            base.InitData(datas);


            combat1 = AddChildren<Combat>();
            combat2 = AddChildren<Combat>();
            combat1.GetComponent<SpellAttackComponent>().SpellAttackWithTarget(combat2);




            var item = combat1.AttachItem(UnityEngine.Resources.Load<ItemConfigObject>("Item_1"));
            //�ͷ��չ���ִ���壬ִ���������һִ֡��������Ҫ�ȴ���һ֡ʹ�õ���
            await Timer.Instance.WaitAsync(1000);
            combat1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combat2);

            combat1.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("ս��1 �յ��˺�" + damageAction.damageValue);
            });
            combat2.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("ս��2 �յ��˺�" + damageAction.damageValue);
            });

            combat1.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("ս��1 �յ�����" + damageAction.cureValue);
            });
            combat2.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("ս��2 �յ�����" + damageAction.cureValue);
            });
        }
    }
}
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
            //释放普攻有执行体，执行体会在下一帧执行所以需要等待下一帧使用道具
            await Timer.Instance.WaitAsync(1000);
            combat1.GetComponent<SpellItemComponent>().SpellItemWithTarget(item, combat2);

            combat1.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("战斗1 收到伤害" + damageAction.damageValue);
            });
            combat2.ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                LogUtil.Debug("战斗2 收到伤害" + damageAction.damageValue);
            });

            combat1.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("战斗1 收到治疗" + damageAction.cureValue);
            });
            combat2.ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                LogUtil.Debug("战斗2 收到治疗" + damageAction.cureValue);
            });
        }
    }
}
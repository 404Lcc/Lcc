using cfg;

namespace LccHotfix
{
    /// <summary>
    /// 参与伤害计算需要的属性
    /// </summary>
    public struct DamageProp
    {
        public long entityId;
        public float atk; //攻击力
        public float def; //防御力
        public float criticalDamage; //暴击伤害
        public float criticalRate; //暴击率
        public float maxHP; //最大血量

        public void Init(long attackerId)
        {
            var entity = EntityUtility.GetEntity(attackerId);
            if (entity == null)
                return;

            if (!entity.hasComID)
                return;

            entityId = attackerId;

            var comProp = entity.comProperty;

            atk = comProp.attack;
            def = comProp.GetFloat(ValuePropertyType.Defence);
            criticalDamage = comProp.criticalDamage;
            criticalRate = comProp.criticalRate;
            maxHP = comProp.maxHP;
        }
    }
}
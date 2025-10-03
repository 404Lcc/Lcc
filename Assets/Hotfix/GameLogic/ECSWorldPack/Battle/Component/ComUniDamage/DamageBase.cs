using cfg;
using System;
using UnityEngine;

namespace LccHotfix
{
    public class DamageBase : IDamage
    {
        public void ApplyDamageByRate(long attackerId, long defenderId, float rate, DamageSource source, KVContext context = null)
        {
            DamageProp prop = new DamageProp();
            prop.Init(attackerId);


            CalcDamageInfo info = new CalcDamageInfo();
            info.damageProp = prop;
            info.defenderId = defenderId;

            var defender = EntityUtility.GetEntity(defenderId);
            if (defender == null)
                return;

            float atk = prop.atk;
            float def = defender.comProperty.GetFloat(ValuePropertyType.Defence);
            float damageBase = atk - def;

            if (damageBase <= 0 && atk > 1)
            {
                damageBase = 1;
            }

            info.baseValue = damageBase;
            info.damageRate = GetDamageRate(prop);
            info.source = source;

            if (context == null)
            {
                info.context = new KVContext();
            }
            else
            {
                info.context = context;
            }

            ApplyChange(info);
            info.context.Clear();
        }

        public void ApplyDamageByFixed(long attackerId, long defenderId, float fixedValue, DamageSource source, KVContext context = null)
        {
            DamageProp prop = new DamageProp();
            prop.Init(attackerId);

            CalcDamageInfo info = new CalcDamageInfo();
            info.damageProp = prop;
            info.defenderId = defenderId;

            info.baseValue = fixedValue;
            info.damageRate = 1;
            info.source = source;

            if (context == null)
            {
                info.context = new KVContext();
            }
            else
            {
                info.context = context;
            }

            ApplyChange(info);
            info.context.Clear();
        }



        public void ApplyRecoverByRate(long attackerId, long defenderId, float rate, DamageSource source, KVContext context = null)
        {
            DamageProp prop = new DamageProp();
            prop.Init(attackerId);

            CalcDamageInfo info = new CalcDamageInfo();
            info.damageProp = prop;
            info.defenderId = defenderId;

            info.baseValue = -Math.Abs(prop.maxHP);
            info.damageRate = rate;
            info.source = source;

            if (context == null)
            {
                info.context = new KVContext();
            }
            else
            {
                info.context = context;
            }

            ApplyChange(info);
            info.context.Clear();
        }

        public void ApplyRecoverByFixed(long attackerId, long defenderId, float fixedValue, DamageSource source, KVContext context = null)
        {
            DamageProp prop = new DamageProp();
            prop.Init(attackerId);

            CalcDamageInfo info = new CalcDamageInfo();
            info.damageProp = prop;
            info.defenderId = defenderId;

            info.baseValue = -Math.Abs(fixedValue);
            info.damageRate = 1;
            info.source = source;

            if (context == null)
            {
                info.context = new KVContext();
            }
            else
            {
                info.context = context;
            }

            ApplyChange(info);
            info.context.Clear();
        }




        public virtual float ApplyChange(CalcDamageInfo info)
        {
            var defenderId = info.defenderId;
            var resultValue = 0f;
            var defender = EntityUtility.GetEntity(defenderId);
            if (defender == null)
                return resultValue;

            if (!defender.hasComProperty)
                return resultValue;

            if (!defender.hasComHP)
                return resultValue;

            if (defender.comHP.HP <= 0)
                return resultValue;

            var damageType = DamageType.Damage;
            if (info.baseValue < 0)
            {
                damageType = DamageType.Recover;
            }

            switch (damageType)
            {
                case DamageType.Damage:
                    resultValue = ApplyDamageChange(info);
                    break;

                case DamageType.Recover:
                    resultValue = ApplyRecoverChange(info);
                    break;
            }

            return resultValue;
        }




        public virtual float ApplyDamageChange(CalcDamageInfo info)
        {
            var resultValue = 0f;
            var attacker = EntityUtility.GetEntity(info.damageProp.entityId);
            var defender = EntityUtility.GetEntity(info.defenderId);

            if (defender == null)
            {
                return resultValue;
            }

            if (!defender.comProperty.isDamageable)
            {
                return resultValue;
            }

            if (defender.hasComDeath)
            {
                return resultValue;
            }

            resultValue = info.ResultValue;
            if (resultValue == 0)
            {
                return resultValue;
            }


            PreDamageChange(info);
            defender.comHP.ChangeHP(-resultValue);
            PostDamageChange(info);

            return resultValue;
        }

        public virtual void PreDamageChange(CalcDamageInfo info)
        {
            var defenderId = info.defenderId;
            var defender = EntityUtility.GetEntity(defenderId);

            var isCrit = CalcCrit(info.damageProp);


            if (isCrit)
            {
                info.damageRate *= CalcCritDamageRate(info.damageProp);
            }


        }

        public virtual void PostDamageChange(CalcDamageInfo info)
        {
            var defenderId = info.defenderId;
            var defender = EntityUtility.GetEntity(defenderId);

            AfterChange(info);

            if (defender.comHP.HP <= 0)
            {
                defender.comHP.SetHP(0);
                var defenderProperty = defender.comProperty;

                //可以死亡
                if (defenderProperty.isDieable)
                {
                    //不能受伤
                    defenderProperty.SetBaseBool(BoolPropertyType.IsAlive, false);
                    defenderProperty.SetBaseBool(BoolPropertyType.Damageable, false);
                    defenderProperty.SetBaseBool(BoolPropertyType.Targetable, false);
                    defenderProperty.SetBaseBool(BoolPropertyType.Hitbackable, false);
                    defenderProperty.SetBaseBool(BoolPropertyType.Stunable, false);
                    //进入死亡
                    defender.AddComLife(0);
                }
            }
            else
            {
                //改变血条
            }
        }




        public virtual float ApplyRecoverChange(CalcDamageInfo info)
        {
            var resultValue = 0f;

            var defenderId = info.defenderId;
            var defender = EntityUtility.GetEntity(defenderId);

            if (defender == null)
            {
                return resultValue;
            }

            if (!defender.comProperty.isHealable)
            {
                return resultValue;
            }

            if (defender.hasComDeath)
            {
                return resultValue;
            }

            resultValue = Mathf.Abs(info.ResultValue);
            if (resultValue == 0)
            {
                return resultValue;
            }


            PreRecoverChange(info);

            if (defender.comHP.HP + resultValue >= defender.comProperty.maxHP)
            {
                defender.comHP.SetHP(defender.comProperty.maxHP);
            }
            else
            {
                defender.comHP.ChangeHP(resultValue);
            }

            PostRecoverChange(info);



            return resultValue;
        }

        public virtual void PreRecoverChange(CalcDamageInfo info)
        {

        }

        public virtual void PostRecoverChange(CalcDamageInfo info)
        {
            var defenderId = info.defenderId;
            var defender = EntityUtility.GetEntity(defenderId);

            AfterChange(info);

            if (defender.comHP.HP <= 0)
            {
                defender.comHP.SetHP(0);
                var defenderProperty = defender.comProperty;

                //可以死亡
                if (defenderProperty.isDieable)
                {
                    //不能受伤
                    defenderProperty.SetBaseBool(BoolPropertyType.Damageable, false);
                    //进入死亡
                    defender.AddComLife(0);
                }
            }
            else
            {
                //改变血条
            }
        }



        public virtual void AfterChange(CalcDamageInfo info)
        {
            var defenderId = info.defenderId;
            var defender = EntityUtility.GetEntity(defenderId);

            //展示hud飘字

            if (defender.hasComFSM && defender.hasComProperty)
            {
                var defenderProperty = defender.comProperty;
                if (info.ResultValue > 0)
                {
                    if (defenderProperty.isHitbackable)
                    {
                        //进入受击状态
                    }
                }
            }
        }




        public virtual float GetDamageRate(DamageProp prop)
        {
            return 1;
        }

        public virtual bool CalcCrit(DamageProp prop)
        {
            return false;
        }

        public virtual float CalcCritDamageRate(DamageProp prop)
        {
            return 1 + (prop.criticalDamage / 100f);
        }
    }
}

namespace LccHotfix
{
    // 攻击分类计算器
    public struct AttackCalculCell
    {
        private readonly UnitSource _attacker;
        private readonly UnitSource _defender;
        
        public AttackCalculCell(in UnitSource attacker, in UnitSource defender)
        {
            _attacker = attacker;
            _defender = defender;
        }
        
        // 计算总攻击力
        public double CalculateTotalAttack()
        {
            double baseAttack = GetAttack();
            return baseAttack;
            // double staticBonus = CalculateStaticBonus(baseAttack);
            // double targetSpecificBonus = CalculateTargetSpecificBonus(baseAttack);
            // double dynamicBonus = CalculateDynamicBonus(baseAttack);
            // return baseAttack + staticBonus + targetSpecificBonus + dynamicBonus;
        }
        
        // 获取攻击
        private double GetAttack()
        {
            var atk = _attacker.Properties.Attack;
            var atkRatio = (1f + _attacker.Properties.ScaleAtk / 10000f);
            if (atkRatio < 0f)
            {
                atkRatio = 0f;
            }

            if (BattleLogger.IsDebugEnabled)
            {
                BattleLogger.LogDebug($"FighterTid={_attacker.FighterTid}, Attack = atk:{atk} * atkRatio:{atkRatio} = {atk * atkRatio}");
            }
            return atk * atkRatio;
        }
    }
}

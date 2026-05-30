namespace LccHotfix
{
    public interface IDamagePropertyModifier
    {
        void ModifyContextProperties(UnitSource attacker, LogicEntity defender, ref DamageContext context);

        double ApplyDamageTypeDamage(in DamageContext context, double baseDamage);
    }

    public partial class LogicWorld
    {
        public IDamagePropertyModifier DamagePropertyModifier { get; private set; }

        public void SetDamagePropertyModifier(IDamagePropertyModifier damagePropertyModifier)
        {
            DamagePropertyModifier = damagePropertyModifier;
        }
    }
}

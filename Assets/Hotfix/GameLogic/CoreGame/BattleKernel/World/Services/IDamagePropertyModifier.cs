namespace LccHotfix
{
    public interface IDamagePropertyModifier
    {
        void ModifyContextProperties(UnitSource attacker, LogicEntity defender, ref DamageContext context);

        double ApplyDamageTypeDamage(in DamageContext context, double baseDamage);
    }
}

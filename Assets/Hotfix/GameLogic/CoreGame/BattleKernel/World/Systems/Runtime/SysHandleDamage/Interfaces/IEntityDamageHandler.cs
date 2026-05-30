namespace LccHotfix
{
    public interface IEntityDamageHandler
    {
        void HandleDamage(DamageContext context, ref DamageResult result);
    }
}

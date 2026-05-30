namespace LccHotfix
{
    public interface IDamageCalculator
    {
        DamageResult Calculate(ref DamageContext context);
    }
}

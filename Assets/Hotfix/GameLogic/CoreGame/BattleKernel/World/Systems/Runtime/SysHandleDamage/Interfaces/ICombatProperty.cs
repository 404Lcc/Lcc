namespace LccHotfix
{
    public interface ICombatProperty
    {
        double GetProperty(int propertyId);
        ICombatProperty SetProperty(int propertyId, double value);
        bool HasProperty(int propertyId);
    }
}

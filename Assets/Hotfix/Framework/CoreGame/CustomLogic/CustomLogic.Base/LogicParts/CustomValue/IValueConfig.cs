namespace LccHotfix
{
    public interface IValueConfig<T>
    {
        T GetDefaultValue();
        T GetValue(CustomNode node);
        bool ParseByFormatString(string s);
    }
}
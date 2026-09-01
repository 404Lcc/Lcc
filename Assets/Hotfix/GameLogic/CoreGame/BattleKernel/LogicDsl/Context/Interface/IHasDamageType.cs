using PBConfig;

namespace LccHotfix
{
    public interface IHasDamageType
    {
        ref TElementType DamageType { get; }
    }
}

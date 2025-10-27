using UnityEngine;

namespace LccHotfix
{
    public interface IIconService : IService
    {
        IconBase GetIcon(IconType type, Transform parent, IconSize size = IconSize.Size_100);
        Vector3 GetIconScale(IconSize size);
    }
}
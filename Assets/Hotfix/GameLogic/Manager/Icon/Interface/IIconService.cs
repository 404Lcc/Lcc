using UnityEngine;

namespace LccHotfix
{
    public interface IIconService : IService
    {
        IconBase GetIcon(IconType type, Transform parent, IconSize size = IconSize.Size_100);

        /// <summary>
        /// icon回收接口
        /// </summary>
        void RecycleIcon<T>(T iconBase) where T : IconBase;

        Vector3 GetIconScale(IconSize size);
    }
}
using UnityEngine;

namespace LccHotfix
{
    public interface IIconService : IService
    {
        T GetIcon<T>(Transform parent, float size = 1) where T : IconBase, new();
    }
}
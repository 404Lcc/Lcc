using UnityEngine;

namespace LccHotfix
{
    public interface IFxService : IService
    {
        FxOne Create(string path, Vector3 pos, float during = -999f, int maxCount = 0);
        FxOne Create(string path, Transform parent, float during = -999f);
        void ClearAll();
    }
}
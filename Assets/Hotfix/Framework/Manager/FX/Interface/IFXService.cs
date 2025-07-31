using UnityEngine;

namespace LccHotfix
{
    public interface IFXService : IService
    {
        FXObject GetFX(int id);

        void Release(int id);

        void Release(FXObject fx);

        void ReleaseAll();

        long PlayNormal(string path, Vector3 pos, Quaternion rot, Vector3 scale, float during, bool ignoreTimeScale = false);

        long PlayNormal(string path, Transform followTrans, float during, bool unit, bool ignoreTimeScale = false);


        long PlayLineRender(string path, Transform fromTrans, Transform toTrans, float during, bool follow, bool ignoreTimeScale = false);
    }
}
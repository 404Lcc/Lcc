// using UnityEngine;
//
// namespace LccHotfix
// {
//     public interface IFXService : IService
//     {
//         FXObject GetFX(int id);
//
//         void Release(int id);
//
//         void Release(FXObject fx);
//
//         void ReleaseAll();
//
//         int PlayNormal(string path, Vector3 pos, Quaternion rot, Vector3 scale, float during, bool ignoreTimeScale = false);
//
//         int PlayNormal(string path, Transform followTrans, float during, bool unit, bool ignoreTimeScale = false);
//         
//         int PlayLineRender(string path, Transform fromTrans, Transform toTrans, float during, bool follow, bool ignoreTimeScale = false);
//     }
// }
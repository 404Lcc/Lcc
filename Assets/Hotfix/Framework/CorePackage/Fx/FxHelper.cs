// using UnityEngine;
//
// namespace LccHotfix
// {
//     public struct CreateFxInfo
//     {
//         public string fxPath;
//         public float during;
//         public int maxCount;
//         public int cost;
//         public int costLimitLevel;
//         public bool isAsyncLoad;
//
//         public Vector3 pos;
//         public Quaternion rot;
//     }
//
//     public class FxHelper
//     {
//         public static FxOne CreateFxByFxInfo(CreateFxInfo fxInfo)
//         {
//             FxCacheManager fxMgr = FxCacheManager.GetInstance();
//             var fxOne = fxMgr.RequestFx_And_Play(EFxOneType.ParticleSystem, fxInfo.fxPath, fxInfo.during,
//                 fxInfo.maxCount, fxInfo.cost, fxInfo.costLimitLevel, fxInfo.isAsyncLoad);
//             if (fxOne == null)
//             {
//                 return null;
//             }
//
//             fxOne.transform.position = fxInfo.pos;
//             fxOne.transform.rotation = fxInfo.rot;
//
//             return fxOne;
//         }
//     }
// }
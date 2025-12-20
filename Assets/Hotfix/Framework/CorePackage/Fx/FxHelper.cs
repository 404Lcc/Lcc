using cfg;
using UnityEngine;

namespace LccHotfix
{
    public struct CreateFxInfo
    {
        public int fxTid;
        public float during;
        public Vector3 pos;
        public Quaternion rot;
    }

    public class FxHelper
    {
        public static FxOne CreateFxByFxInfo(int fxTid, float duration, Vector3 position, Quaternion? rotation = null)
        {
            AssetFx fxConfig = Main.ConfigService.Tables.TBAssetFx.Get(fxTid);

            IFxService fxMgr = Main.FxService;
            var fxOne = fxMgr.RequestFx_And_Play(EFxOneType.ParticleSystem, fxConfig.PathName, duration,
                fxConfig.CountLimit, fxConfig.Cost, (int)fxConfig.FxLevel, fxConfig.AsyncLoad);
            if (fxOne == null)
            {
                return null;
            }
        
            fxOne.transform.position = position;
        
            if (rotation != null)
            {
                fxOne.transform.rotation = (Quaternion)rotation;
            }
        
            return fxOne;
        }
        
        public static FxOne CreateFxByFxInfo(CreateFxInfo fxInfo)
        {
            AssetFx fxConfig = Main.ConfigService.Tables.TBAssetFx.Get(fxInfo.fxTid);
            
            IFxService fxMgr = Main.FxService;
            var fxOne = fxMgr.RequestFx_And_Play(EFxOneType.ParticleSystem, fxConfig.PathName, fxInfo.during,
                fxConfig.CountLimit, fxConfig.Cost, (int)fxConfig.FxLevel, fxConfig.AsyncLoad);
            if (fxOne == null)
            {
                return null;
            }
        
            fxOne.transform.position = fxInfo.pos;
            fxOne.transform.rotation = fxInfo.rot;
        
            return fxOne;
        }
    }
}
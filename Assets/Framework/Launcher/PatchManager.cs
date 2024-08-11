using System.Collections;
using YooAsset;

namespace LccModel
{
    public class PatchManager : SingletonMono<PatchManager>
    {
        public GlobalConfig globalConfig;


        public IEnumerator StartUpdate(GlobalConfig globalConfig)
        {
            this.globalConfig = globalConfig;

            YooAssets.Initialize();


            PatchOperation patchOperation = new PatchOperation(globalConfig.packageName, globalConfig.buildPipeline, globalConfig.playMode);
            YooAssets.StartOperation(patchOperation);

            yield return patchOperation;

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(globalConfig.packageName);
            YooAssets.SetDefaultPackage(gamePackage);

            Loader.Instance.Start(globalConfig);
        }
    }
}
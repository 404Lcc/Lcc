using ET;
using YooAsset;

namespace LccModel
{
    public class PatchManager : AObjectBase
    {
        public static PatchManager Instance { get; set; }



        public GlobalConfig globalConfig;


        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }
        public async ETTask StartUpdate(GlobalConfig globalConfig)
        {
            this.globalConfig = globalConfig;

            YooAssets.Initialize();


            PatchOperation patchOperation = new PatchOperation(globalConfig.packageName, globalConfig.buildPipeline, globalConfig.playMode);
            YooAssets.StartOperation(patchOperation);

            await patchOperation.Task;

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(globalConfig.packageName);
            YooAssets.SetDefaultPackage(gamePackage);

            Loader.Instance.Start(globalConfig);
        }
    }
}
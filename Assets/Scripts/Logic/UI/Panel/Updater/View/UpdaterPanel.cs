namespace LccModel
{
    public class UpdaterPanel : APanelView<UpdaterModel>
    {
        public override void Start()
        {
            base.Start();
            AssetBundleManager.Instance.InitAssets(Message, CopyProgress, DownloadProgress, CheckProgress, Complete, Error).Coroutine();
        }
        public void Message(string message)
        {
            LogUtil.Log(message);
        }
        public void CopyProgress(int currentCopyCount, int copyCount)
        {
        }
        public void DownloadProgress(int currentUpdateCount, int updateCount)
        {
        }
        public void CheckProgress(int currentCheckCount, int checkCount)
        {
        }
        public void Complete()
        {
            ConfigManager.Instance.InitManager();
#if ILRuntime
            ILRuntimeManager.Instance.InitManager();
#else
            MonoManager.Instance.InitManager();
#endif
            ClearPanel();
        }
        public void Error(DownloadData downloadData, string error)
        {
            LogUtil.Log(error);
        }
    }
}
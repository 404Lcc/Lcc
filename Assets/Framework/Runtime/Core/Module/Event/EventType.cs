namespace LccModel
{
    public enum EventType : int
    {
        InitializeFailed,//补丁包初始化失败
        PatchStatesChange,//补丁流程步骤改变
        FoundUpdateFiles,//发现更新文件
        DownloadProgressUpdate,//下载进度更新
        PackageVersionUpdateFailed,//资源版本号更新失败
        PatchManifestUpdateFailed,//补丁清单更新失败
        WebFileDownloadFailed,//网络文件下载失败
    }
}
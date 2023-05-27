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

        UserTryInitialize,//用户尝试再次初始化资源包
        UserBeginDownloadWebFiles,//用户开始下载网络文件
        UserTryUpdatePackageVersion,//用户尝试再次更新静态版本
        UserTryUpdatePatchManifest,//用户尝试再次更新补丁清单
        UserTryDownloadWebFiles,//用户尝试再次下载网络文件
    }
}
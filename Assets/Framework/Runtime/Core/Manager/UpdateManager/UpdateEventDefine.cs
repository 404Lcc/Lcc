namespace LccModel
{
    public class UpdateEventDefine
    {
        /// <summary>
        /// 补丁包初始化失败
        /// </summary>
        public class InitializeFailed
        {
            public static void Publish()
            {
                var msg = new InitializeFailed();
                Event.Instance.Publish(EventType.InitializeFailed, msg);
            }
        }

        /// <summary>
        /// 补丁流程步骤改变
        /// </summary>
        public class PatchStatesChange
        {
            public string Tips;

            public static void Publish(string tips)
            {
                var msg = new PatchStatesChange();
                msg.Tips = tips;
                Event.Instance.Publish(EventType.PatchStatesChange, msg);
            }
        }

        /// <summary>
        /// 发现更新文件
        /// </summary>
        public class FoundUpdateFiles
        {
            public int TotalCount;
            public long TotalSizeBytes;

            public static void Publish(int totalCount, long totalSizeBytes)
            {
                var msg = new FoundUpdateFiles();
                msg.TotalCount = totalCount;
                msg.TotalSizeBytes = totalSizeBytes;
                Event.Instance.Publish(EventType.FoundUpdateFiles, msg);
            }
        }

        /// <summary>
        /// 下载进度更新
        /// </summary>
        public class DownloadProgressUpdate
        {
            public int TotalDownloadCount;
            public int CurrentDownloadCount;
            public long TotalDownloadSizeBytes;
            public long CurrentDownloadSizeBytes;

            public static void Publish(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
            {
                var msg = new DownloadProgressUpdate();
                msg.TotalDownloadCount = totalDownloadCount;
                msg.CurrentDownloadCount = currentDownloadCount;
                msg.TotalDownloadSizeBytes = totalDownloadSizeBytes;
                msg.CurrentDownloadSizeBytes = currentDownloadSizeBytes;
                Event.Instance.Publish(EventType.DownloadProgressUpdate, msg);
            }
        }

        /// <summary>
        /// 资源版本号更新失败
        /// </summary>
        public class PackageVersionUpdateFailed
        {
            public static void Publish()
            {
                var msg = new PackageVersionUpdateFailed();
                Event.Instance.Publish(EventType.PackageVersionUpdateFailed, msg);
            }
        }

        /// <summary>
        /// 补丁清单更新失败
        /// </summary>
        public class PatchManifestUpdateFailed
        {
            public static void Publish()
            {
                var msg = new PatchManifestUpdateFailed();
                Event.Instance.Publish(EventType.PatchManifestUpdateFailed, msg);
            }
        }

        /// <summary>
        /// 网络文件下载失败
        /// </summary>
        public class WebFileDownloadFailed
        {
            public string FileName;
            public string Error;

            public static void Publish(string fileName, string error)
            {
                var msg = new WebFileDownloadFailed();
                msg.FileName = fileName;
                msg.Error = error;
                Event.Instance.Publish(EventType.WebFileDownloadFailed, msg);
            }
        }
    }
}
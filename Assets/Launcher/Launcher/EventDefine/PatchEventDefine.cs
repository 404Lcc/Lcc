using YooAsset;

namespace LccModel
{
    public class PatchEventDefine
    {
        /// <summary>
        /// 补丁包初始化失败
        /// </summary>
        public class InitializeFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new InitializeFailed();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 补丁流程步骤改变
        /// </summary>
        public class PatchStepsChange : IEventMessage
        {
            public string Tips;

            public static void SendEventMessage(string tips)
            {
                var msg = new PatchStepsChange();
                msg.Tips = tips;
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 发现更新文件
        /// </summary>
        public class FoundUpdateFiles : IEventMessage
        {
            public int TotalCount;
            public long TotalSizeBytes;

            public static void SendEventMessage(int totalCount, long totalSizeBytes)
            {
                var msg = new FoundUpdateFiles();
                msg.TotalCount = totalCount;
                msg.TotalSizeBytes = totalSizeBytes;
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 下载进度更新
        /// </summary>
        public class DownloadUpdate : IEventMessage
        {
            public int TotalDownloadCount;
            public int CurrentDownloadCount;
            public long TotalDownloadSizeBytes;
            public long CurrentDownloadSizeBytes;

            public static void SendEventMessage(DownloadUpdateData updateData)
            {
                var msg = new DownloadUpdate();
                msg.TotalDownloadCount = updateData.TotalDownloadCount;
                msg.CurrentDownloadCount = updateData.CurrentDownloadCount;
                msg.TotalDownloadSizeBytes = updateData.TotalDownloadBytes;
                msg.CurrentDownloadSizeBytes = updateData.CurrentDownloadBytes;
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 资源版本请求失败
        /// </summary>
        public class PackageVersionRequestFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PackageVersionRequestFailed();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 资源清单更新失败
        /// </summary>
        public class PackageManifestUpdateFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PackageManifestUpdateFailed();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 网络文件下载失败
        /// </summary>
        public class WebFileDownloadFailed : IEventMessage
        {
            public string FileName;
            public string Error;

            public static void SendEventMessage(DownloadErrorData errorData)
            {
                var msg = new WebFileDownloadFailed();
                msg.FileName = errorData.FileName;
                msg.Error = errorData.ErrorInfo;
                Event.SendMessage(msg);
            }
        }
    }
}
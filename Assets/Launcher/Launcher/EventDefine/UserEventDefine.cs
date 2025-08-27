namespace LccModel
{
    public class UserEventDefine
    {
        /// <summary>
        /// 用户尝试再次初始化资源包
        /// </summary>
        public class UserTryInitialize : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryInitialize();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 用户开始下载网络文件
        /// </summary>
        public class UserBeginDownloadWebFiles : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserBeginDownloadWebFiles();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 用户尝试再次请求资源版本
        /// </summary>
        public class UserTryRequestPackageVersion : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryRequestPackageVersion();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 用户尝试再次更新补丁清单
        /// </summary>
        public class UserTryUpdatePackageManifest : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryUpdatePackageManifest();
                Event.SendMessage(msg);
            }
        }

        /// <summary>
        /// 用户尝试再次下载网络文件
        /// </summary>
        public class UserTryDownloadWebFiles : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryDownloadWebFiles();
                Event.SendMessage(msg);
            }
        }
    }
}
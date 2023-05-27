namespace LccModel
{
	public class UserEventDefine
	{
		/// <summary>
		/// 用户尝试再次初始化资源包
		/// </summary>
		public class UserTryInitialize
		{
			public static void Publish()
			{
				var msg = new UserTryInitialize();
				Event.Instance.Publish(EventType.UserTryInitialize, msg);
			}
		}

		/// <summary>
		/// 用户开始下载网络文件
		/// </summary>
		public class UserBeginDownloadWebFiles
		{
			public static void Publish()
			{
				var msg = new UserBeginDownloadWebFiles();
				Event.Instance.Publish(EventType.UserBeginDownloadWebFiles, msg);
			}
		}

		/// <summary>
		/// 用户尝试再次更新静态版本
		/// </summary>
		public class UserTryUpdatePackageVersion
		{
			public static void Publish()
			{
				var msg = new UserTryUpdatePackageVersion();
				Event.Instance.Publish(EventType.UserTryUpdatePackageVersion, msg);
			}
		}

		/// <summary>
		/// 用户尝试再次更新补丁清单
		/// </summary>
		public class UserTryUpdatePatchManifest
		{
			public static void Publish()
			{
				var msg = new UserTryUpdatePatchManifest();
				Event.Instance.Publish(EventType.UserTryUpdatePatchManifest, msg);
			}
		}

		/// <summary>
		/// 用户尝试再次下载网络文件
		/// </summary>
		public class UserTryDownloadWebFiles
		{
			public static void Publish()
			{
				var msg = new UserTryDownloadWebFiles();
				Event.Instance.Publish(EventType.UserTryDownloadWebFiles, msg);
			}
		}
	}
}
using System;
using System.IO;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
	/// <summary>
	/// 初始化资源包
	/// </summary>
	public class FsmInitialize : IStateNode
	{
		private StateMachine _machine;

		public void OnCreate(StateMachine machine)
		{
			_machine = machine;
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("初始化资源包！");
			UpdateManager.Instance.StartCoroutine(InitPackage());
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private IEnumerator InitPackage()
		{
			yield return new WaitForSeconds(1f);

			var playMode = UpdateManager.Instance.PlayMode;

			// 创建默认的资源包
			string packageName = UpdateManager.DefaultPackage;
			var package = YooAssets.TryGetPackage(packageName);
			if (package == null)
			{
				package = YooAssets.CreatePackage(packageName);
				YooAssets.SetDefaultPackage(package);
			}

			// 编辑器下的模拟模式
			InitializationOperation initializationOperation = null;
			if (playMode == EPlayMode.EditorSimulateMode)
			{
				var createParameters = new EditorSimulateModeParameters();
				createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
				initializationOperation = package.InitializeAsync(createParameters);
			}

			// 单机运行模式
			if (playMode == EPlayMode.OfflinePlayMode)
			{
				var createParameters = new OfflinePlayModeParameters();
				createParameters.DecryptionServices = new GameDecryptionServices();
				initializationOperation = package.InitializeAsync(createParameters);
			}

			// 联机运行模式
			if (playMode == EPlayMode.HostPlayMode)
			{
				var createParameters = new HostPlayModeParameters();
				createParameters.DecryptionServices = new GameDecryptionServices();
				createParameters.QueryServices = new GameQueryServices();
				createParameters.DefaultHostServer = GetHostServerURL();
				createParameters.FallbackHostServer = GetHostServerURL();
				initializationOperation = package.InitializeAsync(createParameters);
			}

			yield return initializationOperation;
			if (initializationOperation.Status == EOperationStatus.Succeed)
			{
				_machine.ChangeState<FsmUpdateVersion>();
			}
			else
			{
				Debug.LogWarning($"{initializationOperation.Error}");
				UpdateEventDefine.InitializeFailed.Publish();
			}
		}

		/// <summary>
		/// 获取资源服务器地址
		/// </summary>
		private string GetHostServerURL()
		{
			string hostServerIP = UpdateManager.Instance.GetHostServer();
			string gameVersion = UpdateManager.Instance.GetVersion();

#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
				return $"{hostServerIP}/CDN/Android/{gameVersion}";
			else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
				return $"{hostServerIP}/CDN/IPhone/{gameVersion}";
			else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
				return $"{hostServerIP}/CDN/WebGL/{gameVersion}";
			else
				return $"{hostServerIP}/CDN/PC/{gameVersion}";
#else
		if (Application.platform == RuntimePlatform.Android)
			return $"{hostServerIP}/CDN/Android/{gameVersion}";
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
			return $"{hostServerIP}/CDN/IPhone/{gameVersion}";
		else if (Application.platform == RuntimePlatform.WebGLPlayer)
			return $"{hostServerIP}/CDN/WebGL/{gameVersion}";
		else
			return $"{hostServerIP}/CDN/PC/{gameVersion}";
#endif
		}

		/// <summary>
		/// 内置文件查询服务类
		/// </summary>
		private class GameQueryServices : IQueryServices
		{
			public bool QueryStreamingAssets(string fileName)
			{
				string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
				return StreamingAssetsUtil.FileExists($"{buildinFolderName}/{fileName}");
			}
		}

		/// <summary>
		/// 资源文件解密服务类
		/// </summary>
		private class GameDecryptionServices : IDecryptionServices
		{
			public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
			{
				return 32;
			}

			public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
			{
				throw new NotImplementedException();
			}

			public Stream LoadFromStream(DecryptFileInfo fileInfo)
			{
				BundleStream bundleStream = new BundleStream(fileInfo.FilePath, FileMode.Open);
				return bundleStream;
			}

			public uint GetManagedReadBufferSize()
			{
				return 1024;
			}
		}
	}
}
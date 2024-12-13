using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
	public class ResShader : ResObject
	{
		private static Dictionary<string, Shader> _shaderDict = new Dictionary<string, Shader>();

		public static Shader FindShader(string shaderName)
		{
			_shaderDict.TryGetValue(shaderName, out Shader shader);
			return shader;
		}

		protected override void Load()
		{
			bool valid = AssetManager.Instance.CheckLocationValid(_assetName);
			if (!valid)
			{
				_state = LoadState.Error;
				LoadEnd();
				return;
			}
			_handleBase = AssetManager.Instance.LoadAllAssetsSync(_assetName);
			if (LoadShader(_handleBase))
			{
				_state = LoadState.Done;
			}
			else
			{
				_state = LoadState.Error;
			}
			LoadEnd();
		}

		private bool LoadShader(HandleBase handleBase)
		{
			var allAssetsHandle = (AllAssetsHandle)handleBase;

			if (allAssetsHandle.AllAssetObjects != null)
			{
				Object[] allAssets = allAssetsHandle.AllAssetObjects;
				if (allAssets != null && allAssets.Length > 0)
				{
					for (int i = allAssets.Length - 1; i >= 0; i--)
					{
						var obj = allAssets[i];
						if (obj is Shader)
						{
							if (!_shaderDict.ContainsKey(obj.name))
							{
								_shaderDict.Add(obj.name, obj as Shader);
							}
							else
							{
								_shaderDict[obj.name] = obj as Shader;
							}
						}
						else if (obj is ShaderVariantCollection)
						{
							var shaderVariantCollection = obj as ShaderVariantCollection;
							StartCoroutine(WarmUp(shaderVariantCollection));
						}
					}
				}
				return true;
			}
			return false;
		}

		IEnumerator WarmUp(ShaderVariantCollection shaderVariantCollection)
		{
			yield return new WaitForSeconds(1.5f);
			try
			{
				shaderVariantCollection.WarmUp();
				Debug.Log($"worm up shader variantcollection {shaderVariantCollection.name}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"worm up shader variantcollection {shaderVariantCollection.name} with error: {ex.ToString()}");
			}
		}


		#region 加载接口

		public static ResShader LoadShader(GameObject loader, string asset)
		{
			if (loader != null && !string.IsNullOrEmpty(asset))
			{
				ResShader res = loader.AddComponent<ResShader>();
				res.SetInfo(loader, asset, null);
				res.Load();
				return res;
			}
			return null;
		}
		#endregion
	}
}
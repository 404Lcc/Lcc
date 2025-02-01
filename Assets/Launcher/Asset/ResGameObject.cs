using System;
using UnityEngine;

namespace LccModel
{
	public class ResGameObject : ResObject
	{
		// 实例化后保持加载对象处于顶层
		private bool _keepHierar;
		private event Action<string, GameObject> _onGameObjectComplete;
		private GameObject _resGO;
		private Transform _resTransform;

		public GameObject ResGO => _resGO;
		public Transform ResTransform => _resTransform;

		public void DestroyResGameObject()
		{
			if (_resGO != null)
			{
				Destroy(_resGO);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public override void LoadEnd()
		{
			if (_bindObject == null) return;
			if (_obj != null)
			{
				GameObject ins = Instantiate(_obj) as GameObject;
				ins.name = _assetName;

				if (_keepHierar)
				{
					_resTransform = ins.transform;
					_resTransform.SetParent(transform.parent);
					transform.SetParent(_resTransform);
					_resTransform.localScale = Vector3.one;
					_resTransform.localRotation = Quaternion.identity;
					_resTransform.localPosition = Vector3.zero;
					_resGO = ins;
				}
				else
				{
					_resTransform = transform;
					ins.transform.SetParent(_resTransform);
					ins.transform.localScale = Vector3.one;
					ins.transform.localRotation = Quaternion.identity;
					ins.transform.localPosition = Vector3.zero;
					_resGO = gameObject;
				}

#if UNITY_EDITOR
				//为了在编辑器下运行时移动平台的shader能正常显示

				Renderer[] res = _resGO.GetComponentsInChildren<Renderer>();
				foreach (Renderer re in res)
				{
					foreach (var ma in re.materials)
					{
						if (ma != null)
						{
							var shaderName = ma.shader.name;
							var newShader = Shader.Find(shaderName);
							if (newShader != null)
							{
								ma.shader = newShader;
							}
						}
					}
				}
#endif
			}

			base.LoadEnd();

			if (_onGameObjectComplete != null)
			{
				_onGameObjectComplete(_assetName, _resGO);
			}
		}

		#region 加载接口
		public static ResGameObject LoadGameObject(string asset, bool keepHierar = false)
		{
			if (!string.IsNullOrEmpty(asset))
			{
				GameObject ins = new GameObject($"res-{asset}");
				ResGameObject res = ins.AddComponent<ResGameObject>();

				res.SetInfo<GameObject>(ins, asset, null);
				res._keepHierar = keepHierar;
				res.Load();
				return res;
			}
			return null;
		}

		public static ResGameObject StartLoadGameObject(GameObject loader, string asset, Action<string, GameObject> onComplete, bool keepHierar = false)
		{
			if (loader != null && !string.IsNullOrEmpty(asset))
			{
				GameObject ins = new GameObject($"res-{asset}");
				ins.transform.SetParent(loader.transform);
				ResGameObject res = ins.AddComponent<ResGameObject>();

				res.SetInfo<GameObject>(ins, asset, null);
				res._keepHierar = keepHierar;
				res._onGameObjectComplete = onComplete;
				res.StartLoad();
				return res;
			}
			return null;
		}
		#endregion
	}
}
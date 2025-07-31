using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class ResALLObject : ResObject
    {
        private bool _isLoadingAssets;
        private Dictionary<string, Object> _assetDict = new Dictionary<string, Object>();

        public Dictionary<string, Object> FindObjects()
        {
            return _assetDict;
        }

        protected override void StartLoad()
        {
            bool valid = Main.AssetService.CheckLocationValid(_assetName);
            if (!valid)
            {
                _state = LoadState.Error;
                LoadEnd();
                return;
            }

            _state = LoadState.Loading;
            _loadStartTime = Time.realtimeSinceStartup;

            _handleBase = Main.AssetService.LoadAllAssetsAsync(_assetName, _type);
            ((AllAssetsHandle)_handleBase).Completed += OnCompleted;
        }

        private void OnCompleted(AllAssetsHandle allAssetsHandle)
        {
            _isLoadingAssets = true;
            try
            {
                Object[] allAssets = allAssetsHandle.AllAssetObjects;
                if (allAssets != null && allAssets.Length > 0)
                {
                    for (int i = allAssets.Length - 1; i >= 0; i--)
                    {
                        if (!_assetDict.ContainsKey(allAssets[i].name))
                        {
                            _assetDict.Add(allAssets[i].name, allAssets[i]);
                        }
                        else
                        {
                            _assetDict[allAssets[i].name] = allAssets[i];
                        }
                    }
                }
                _state = LoadState.Done;
            }
            catch
            {
                _state = LoadState.Error;
            }
            finally
            {
                _isLoadingAssets = false;
            }
        }

        #region 加载接口
        public static ResALLObject LoadObjects(GameObject loader, string asset)
        {
            if (loader != null && !string.IsNullOrEmpty(asset))
            {
                ResALLObject res = null;
                ResALLObject[] old = loader.GetComponents<ResALLObject>();
                if (old != null && old.Length > 0)
                {
                    if (old != null && old.Length > 0)
                    {
                        for (int i = 0; i < old.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(old[i]._assetName) && old[i]._assetName.Equals(asset))
                            {
                                return old[i];
                            }
                        }
                        for (int i = 0; i < old.Length; i++)
                        {
                            // 空的没用的对象，一般通过复制对象copy过来的
                            if (string.IsNullOrEmpty(old[i]._assetName))
                            {
                                res = old[i];
                                res._assetDict.Clear();
                            }
                        }

                    }
                }

                if (res == null)
                {
                    res = loader.AddComponent<ResALLObject>();
                }
                res.SetInfo<Object>(loader, asset, null);
                res.StartLoad();
                return res;
            }
            return null;
        }
        #endregion
    }
}
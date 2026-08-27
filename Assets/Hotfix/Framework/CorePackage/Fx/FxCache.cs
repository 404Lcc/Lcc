using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class FxCache : MonoBehaviour
    {
        string _path;
        // 池键，与 FxCacheManager 字典 key 一致
        public string Path => _path;
        int _cost = 1;
        int _maxCount;
        int _usedCount;
        EFxOneType _fxType;

        private List<FxOne> _cachedFxOneList;

        private AssetHandle _assetHandle;
        private bool _bLoading;
        private bool _bDestroyed;

        public void Awake()
        {
            _bLoading = true;
            _bDestroyed = false;
        }

        ~FxCache()
        {
            _bLoading = true;
            _bDestroyed = true;
        }

        public void InitCache(EFxOneType fxType, string path, int cost, int capacity, int maxCount, bool isAsyncLoad)
        {
            _fxType = fxType;
            _path = path;
            _cost = cost;
            _maxCount = maxCount;
            capacity = Math.Min(capacity, maxCount);

            _cachedFxOneList = new List<FxOne>(capacity);
            for (int i = 0; i < capacity; ++i)
                CreateEmptyFxToTail();

            if (isAsyncLoad)
            {
                Main.AssetService.LoadAssetAsync(path, OnLoadResourceTemplate);
            }
            else
            {
                var asset = Main.AssetService.LoadAssetSync(path);
                if (asset != null)
                {
                    OnLoadResourceTemplate(asset);
                }
            }
        }

        private void CreateEmptyFxToTail()
        {
            GameObject go = new GameObject($"FxOne_{_path}_{_cachedFxOneList.Count}");
            go.transform.parent = transform;
            FxOne fxOne = go.AddComponent<FxOne>();
            fxOne.fxCache = this;

            fxOne.SetHiddenInGame(true);

            if (!_bLoading)
            {
                _CreateFxGameObject(fxOne);
            }

            _cachedFxOneList.Add(fxOne);
        }

        private void OnLoadResourceTemplate(AssetHandle assetHandle)
        {
            if (assetHandle == null || _bDestroyed)
                return;

            _bLoading = false;
            _assetHandle = assetHandle;

            foreach (var cachedFxOne in _cachedFxOneList)
            {
                _CreateFxGameObject(cachedFxOne);
            }
        }

        private void _CreateFxGameObject(FxOne fxOne)
        {
            if (_assetHandle == null || fxOne == null)
                return;

            // 实例化一个特效的 GameObject，并放在 FxOne 下
            GameObject newFxObject = _assetHandle.InstantiateSync(fxOne.transform);
            if (newFxObject == null)
                return;

            fxOne.SetFxGameObject(newFxObject);
        }

        /// <summary>
        /// 内容 GO 被 DestructionEffect Destroy 后，按模板重新实例化。
        /// </summary>
        public void RecreateFxGameObject(FxOne fxOne)
        {
            if (fxOne == null || _assetHandle == null || _bDestroyed)
                return;

            _CreateFxGameObject(fxOne);
        }

        public void ExpandCache(int newCapacity)
        {
            newCapacity = Math.Min(newCapacity, _maxCount);
            int curCapacity = _cachedFxOneList.Capacity;

            if (curCapacity < newCapacity)
            {
                for (int i = curCapacity; i < newCapacity; ++i)
                    CreateEmptyFxToTail();
            }
        }

        public FxOne RequestFx()
        {
            if (_maxCount > 0 && _usedCount >= _maxCount)
                return null;

            int curCount = _cachedFxOneList.Count;
            if (_usedCount >= curCount)
            {
                CreateEmptyFxToTail();
            }

            FxOne newFx = _cachedFxOneList[_usedCount];
            newFx.SetHiddenInGame(false);
            newFx.bIsReleased = false;
            // 防止上一轮残留的分级回调误触发
            newFx.OnReleased = null;
            newFx.VfxGradePath = null;
            ++_usedCount;

            return newFx;
        }
        
        public void ReleaseFx(FxOne fXOne)
        {
            if (fXOne == null)
                return;

            int index = _cachedFxOneList.IndexOf(fXOne);
            ReleaseFx(index);
        }

        public void ReleaseFx(int index)
        {
            if (index < 0 || index >= _usedCount)
                return;

            FxOne toRelease = _cachedFxOneList[index];
            FxOne lastUsedFx = _cachedFxOneList[_usedCount - 1];
            _cachedFxOneList[index] = lastUsedFx;
            _cachedFxOneList[_usedCount - 1] = toRelease;

            toRelease.transform.localScale = Vector3.one;
            toRelease.SetHiddenInGame(true);
            toRelease.bIsReleased = true;

            --_usedCount;
        }
        
        public void ReleaseAllFx()
        {
            // 走 FxOne.Release，确保 OnReleased（分级占位）被触发
            for (int i = _usedCount - 1; i >= 0; i--)
            {
                var fx = _cachedFxOneList[i];
                if (fx != null && !fx.bIsReleased)
                    fx.Release();
                else
                    ReleaseFx(i);
            }
        }

        // 销毁本池：归还在用槽位、Destroy 池节点（含 FxOne）
        // AssetHandle 由 AssetManager/AssetLoader 持有，此处不 Release，避免字典残留已释放句柄导致下局 InstantiateSync 失败
        public void DisposeCache()
        {
            if (_bDestroyed)
            {
                return;
            }

            ReleaseAllFx();

            _bDestroyed = true;
            _assetHandle = null;
            _cachedFxOneList?.Clear();
            Destroy(gameObject);
        }

        #region Cost

        public int GetCurCost()
        {
            return _cost * _usedCount;
        }

        #endregion
    }
}
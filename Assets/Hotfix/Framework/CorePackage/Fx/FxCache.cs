using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class FxCache : MonoBehaviour
    {
        string _path;
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
            if (_assetHandle == null)
                return;

            // 实例化一个特效的 GameObject，并放在 FxOne 下
            GameObject newFxObject = _assetHandle.InstantiateSync(fxOne.transform);
            fxOne.SetFxGameObject(newFxObject);
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
	
            toRelease.SetHiddenInGame(true);
            toRelease.bIsReleased = true;

            --_usedCount;
        }

        #region Cost

        public int GetCurCost()
        {
            return _cost * _usedCount;
        }

        #endregion
    }
}
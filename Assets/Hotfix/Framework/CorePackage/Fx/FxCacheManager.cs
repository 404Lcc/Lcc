using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum EFxOneType
    {
        None = 0,
        ParticleSystem = 1,
        GameObject = 2,
    };

    internal class FxCacheManager : Module, IFxService
    {
        private List<int> CostLimitArray = new List<int>() { 200, 700, 900, 1000 };
        private Dictionary<string, FxCache> fxCaches = new Dictionary<string, FxCache>();

        private uint cacheGuid = 0;

        private Transform _root;
        public FxCacheManager()
        {
            _root = new GameObject("FxCacheRoot").transform;
            GameObject.DontDestroyOnLoad(_root);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        internal override void Shutdown()
        {
            GameObject.Destroy(_root.gameObject);
            _root = null;
        }
        
        #region Preload

        public FxCache LoadFxCache(EFxOneType fxType, string path, int cost, int capacity, int maxCount,
            bool isAsyncLoad)
        {
            if (path.Equals(""))
                return null;

            if (fxCaches.TryGetValue(path, out FxCache fxCache))
            {
                fxCache.ExpandCache(capacity);
            }
            else
            {
                var cacheName = $"FxCache_{path}_{++cacheGuid}";
                GameObject newCacheObject = new GameObject(cacheName)
                {
                    transform =
                    {
                        parent = _root
                    }
                };

                fxCache = newCacheObject.AddComponent<FxCache>();
                fxCaches[path] = fxCache;
                fxCache.InitCache(fxType, path, cost, capacity, maxCount, isAsyncLoad);
            }

            return fxCache;
        }

        #endregion

        /// <summary>
        /// 创建一个特效并播放
        /// </summary>
        /// <param name="fxType">特效类型</param>
        /// <param name="path">特效路径</param>
        /// <param name="during">持续时间</param>
        /// <param name="maxCount">最大创建数量</param>
        /// <param name="cost">特效消耗计数</param>
        /// <param name="costLimitLevel"></param>
        /// <param name="isAsyncLoad">是否为异步加载</param>
        /// <returns></returns>
        public FxOne RequestFx_And_Play(EFxOneType fxType, string path, float during = -1, int maxCount = 0,
            int cost = 0, int costLimitLevel = 0, bool isAsyncLoad = true)
        {
            FxOne fx = RequestFx_With_Cost(fxType, path, cost, maxCount, costLimitLevel, isAsyncLoad);
            if (fx != null)
            {
                fx.Play(during);
            }

            return fx;
        }

        public FxOne RequestFx_With_Cost(EFxOneType fxType, string path, int cost, int maxCount,
            int costLimitLevel, bool isAsyncLoad)
        {
            if (path.Equals("") || CostLimitArray.Count <= 0)
                return null;

            if (costLimitLevel >= CostLimitArray.Count)
                costLimitLevel = CostLimitArray.Count - 1;

            int costLimit = CostLimitArray[costLimitLevel];
            if (GetCurCost() + cost > costLimit)
            {
                return null;
            }

            FxCache fXCache = GetOrCreateCache(fxType, path, cost, 1, maxCount, isAsyncLoad);
            if (fXCache != null)
            {
                return fXCache.RequestFx();
            }

            return null;
        }

        private int GetCurCost()
        {
            int cost = 0;
            foreach (var fxCachePair in fxCaches)
            {
                var fxCache = fxCachePair.Value;
                cost += fxCache.GetCurCost();
            }

            return cost;
        }

        FxCache GetOrCreateCache(EFxOneType fxType, string path, int cost, int capacity, int maxCount, bool isAsyncLoad)
        {
            if (fxCaches.TryGetValue(path, out FxCache fxCache))
            {
                return fxCache;
            }

            return LoadFxCache(fxType, path, cost, capacity, maxCount, isAsyncLoad);
        }
    }
}
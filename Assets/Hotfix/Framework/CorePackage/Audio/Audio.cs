using System;
using YooAsset;
using Newtonsoft.Json;

namespace LccHotfix
{
    /// <summary>
    /// 提供依赖外部调用接口的二次封装，整个音频模块除了此处以外不应依赖外部接口
    /// </summary>
    public static class Audio
    {
        #region log

        public static void Log(string s)
        {
            KLogger.Log(s);
        }

        public static void LogWarning(string s)
        {
            KLogger.LogWarning(s);
        }

        public static void LogError(string s)
        {
            KLogger.LogError(s);
        }

        public static void LogError(Exception e)
        {
            KLogger.LogError(e.Message);
        }

        #endregion

        #region reference pool

        public static IReference PoolAllocate(Type t)
        {
            return ReferencePool.Acquire(t);
        }
        public static T PoolAllocate<T>() where T : class, IReference, new()
        {
            return ReferencePool.Acquire<T>();
        }

        public static void PoolRecycle(IReference o)
        {
            ReferencePool.Release(o);
        }

        #endregion

        #region asset

        public static AssetHandle LoadAssetSync(string path)
        {
            return Main.AssetService.LoadAssetSync(path);
        }

        public static void LoadAssetAsync<T>(string path, Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : UnityEngine.Object
        {
            Main.AssetService.LoadAssetAsync<T>(path, handle =>
            {
                callback(handle);
            }, group, priority);
        }

        public static RawFileHandle LoadRawFileSync(string path)
        {
            return Main.AssetService.LoadAssetRawFileSync(path);
        }

        public static void Release(string path)
        {
            Main.AssetService.Release(path);
        }

        #endregion

        #region serialize

        public static void Deserialize(string s, object obj)
        {
            JsonConvert.PopulateObject(s, obj);
        }

        public static string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        #endregion
    }
}
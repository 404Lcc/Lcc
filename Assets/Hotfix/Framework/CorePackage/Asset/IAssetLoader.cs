using System.Collections;
using YooAsset;

namespace LccHotfix
{
    public interface IAssetLoader
    {
        /// <summary>
        /// 释放所有资源
        /// </summary>
        void Release();

        /// <summary>
        /// 释放指定资源
        /// </summary>
        /// <param name="location">资源定位地址</param>
        void Release(string location);

        /// <summary>
        /// 尝试获取已加载的资源句柄
        /// </summary>
        AssetHandle TryGetAsset(string location);

        #region 异步加载方法

        /// <summary>
        /// 异步加载资源
        /// </summary>
        void LoadAssetAsync(string location, System.Action<AssetHandle> callback, uint priority = 0);

        /// <summary>
        /// 异步加载指定类型资源
        /// </summary>
        void LoadAssetAsync<T>(string location, System.Action<AssetHandle> onCompleted, uint priority = 0) where T : UnityEngine.Object;

        void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> onCompleted, uint priority = 0);

        #endregion

        #region 协程加载方法

        /// <summary>
        /// 协程方式加载资源
        /// </summary>
        IEnumerator LoadAssetCoro(string location, System.Action<AssetHandle> onBegin = null, uint priority = 0);

        /// <summary>
        /// 协程方式加载指定类型资源
        /// </summary>
        IEnumerator LoadAssetCoro<T>(string location, System.Action<AssetHandle> onBegin = null, uint priority = 0) where T : UnityEngine.Object;

        #endregion

        #region 同步加载方法

        /// <summary>
        /// 同步加载资源（仅适用于小资源）
        /// </summary>
        AssetHandle LoadAssetSync(string location);

        /// <summary>
        /// 同步加载指定类型资源（仅适用于小资源）
        /// </summary>
        AssetHandle LoadAssetSync<T>(string location);

        /// <summary>
        /// 同步加载原始文件（仅适用于小资源）
        /// </summary>
        RawFileHandle LoadAssetRawFileSync(string location);

        #endregion
    }
}
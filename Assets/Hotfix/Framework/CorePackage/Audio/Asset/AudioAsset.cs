
using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    /// <summary>
    /// 包装音频资源，方便加载与卸载
    /// </summary>
    public class AudioAsset<T> where T : Object
    {
        public T Asset { get; private set; }
        private AssetHandle _handle;
        private string _path;

        public static implicit operator T(AudioAsset<T> asset)
        {
            return asset.Asset;
        }

        public static AudioAsset<T> Load(string path)
        {
            AssetHandle handle = null;
            AudioAsset<T> audioAsset = new();
            try
            {
                handle = Audio.LoadAssetSync(path);
                if (handle == null)
                {
                    return null;
                }
                audioAsset._handle = handle;
                audioAsset.Asset = handle.AssetObject as T;
                audioAsset._path = path;
            }
            catch (System.Exception ex)
            {
                Audio.LogError($"[Audio] 加载音频资源（ 路径：\"{path}\" ）失败, 信息: {ex.Message}");
            }
            return audioAsset;
        }

        public IEnumerator LoadASync(string path)
        {
            if (_handle is not null)
            {
                yield break;
            }
            Audio.LoadAssetAsync<T>(path, handle =>
            {
                _handle = handle;
            });
            yield return _handle;  
            Asset = _handle.AssetObject as T;
        }
        
        public void Unload()
        {
            if (_handle is null)
            {
                return;
            }
            Audio.Release(_path);
            Asset = null;
            _path = null;
            _handle = null;
        }
    }
}
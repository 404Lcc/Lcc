using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    /// <summary>
    /// 音频元数据
    /// </summary>
    public class AudioRawFile<T> where T : new()
    {
        public T Content { get; private set; }
        public string Path { get; private set; }

        public static implicit operator T(AudioRawFile<T> rawFile)
        {
            return rawFile.Content;
        }

        public static AudioRawFile<T> Load(string path)
        {
            RawFileHandle handle = null;
            AudioRawFile<T> audioRawFile = new();
            try
            {
                handle = Audio.LoadRawFileSync(path);
                if (handle == null)
                {
                    return null;
                }
                audioRawFile.Path = handle.GetRawFilePath();
                audioRawFile.Content = new T();
                Audio.Deserialize(handle.GetRawFileText(), audioRawFile.Content);
            }
            catch (System.Exception ex)
            {
                Audio.LogError($"[Audio] Load rawfile \"{path}\" error, message: {ex.Message}");
            }
            finally
            {
                Audio.Release(path);
            }
            return audioRawFile;
        }

        public void Unload()
        {
            Content = default;
            Path = null;
        }
    }
}
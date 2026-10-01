
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
#endif

namespace LccHotfix
{
    /// <summary>
    /// 音频元数据包装为Asset
    /// </summary>
    public class AudioConfigBase : ScriptableObject
    {
        public static AudioConfigBase Deserialize<T>(string s) where T : AudioConfigBase, new()
        {
            var obj = CreateInstance<T>();
            Audio.Deserialize(s, obj);
            return obj;
        }

#if UNITY_EDITOR
        public void Save(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            var file = new StreamWriter(path, false);
            try
            {
                file.Write(Audio.Serialize(this));
            }
            finally
            {
                file.Close();
                file.Dispose();
            }
        }
#endif
    }
}
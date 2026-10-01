using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// IAudioStereoScheme 立体声方案
    /// 决定了附带有世界坐标的音频应当遵循怎样的规则去调整音量和声像
    /// </summary>
    public interface IAudioStereo
    {
        public float Pan(Vector3 pos);
        public float Volume(Vector3 pos);
    }
}
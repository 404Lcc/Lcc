using System;

namespace LccHotfix
{
    /// <summary>
    /// AudioEvent 对象池工具类
    /// </summary>
    public static class AudioEventPool
    {
        public static AudioEvent CreateEvent(Type type)
        {
            if (!typeof(AudioEvent).IsAssignableFrom(type))
            {
                return null;
            }
            return Audio.PoolAllocate(type) as AudioEvent;
        }

        public static void ReleaseEvent(AudioEvent evt)
        {
            if (evt is null)
            {
                return;
            }
            Audio.PoolRecycle(evt);
        }
    }
}
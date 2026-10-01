using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// AudioEventGroup 时音频的运行时状态，用于存储一组音频的参数
    /// </summary>
    public class AudioEventGroup : IReference
    {
        public int EventCount; // 当前存在的事件数量
        public float LastEventTime = -1; // 距离上一次事件的触发时间

        public void Update()
        {
            LastEventTime += Time.deltaTime;
        }

        public void OnRecycle()
        {
            EventCount = 0;
            LastEventTime = -1;
        }
    }
}
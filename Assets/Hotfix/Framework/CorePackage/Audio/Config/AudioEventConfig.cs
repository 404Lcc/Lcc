using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    [Serializable]
    public class AudioEventConfig
    {
        public string Name; // 名称
        public string TagName; // 组名
        public string Desc; // 注释
        public string Event; // 音频事件
        public List<string> Clips; // 音频剪辑列表
        public float Volume = 1.0f; // 事件音量
        public int MaxStack = 3; // 最大堆叠数
        public float MinInterval = 0.1f; // 最小播放间隔
        public float PitchShift = 0; // 变调范围
        public bool CanClean = true; // 可被清理
        public bool CanPause = true; // 可被暂停
    }
}
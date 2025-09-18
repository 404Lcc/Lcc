using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{

    public interface IAudioService : IService
    {
        // float SoundVolume { get; set; }
        // float MusicVolume{ get; set; }
        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public AudioSource PlaySound(string audio, float volume = 1f, float pitch = 1, bool loop = false);
        public AudioSource PlayMusic(string audio, float volume = 1f, float pitch = 1, bool loop = false);
        /// <summary>
        /// 停止接口
        /// </summary>
        public void StopAudio(string audio);
        /// <summary>
        /// 暂停所有音频
        /// </summary>
        public void PauseAll();
    }
}
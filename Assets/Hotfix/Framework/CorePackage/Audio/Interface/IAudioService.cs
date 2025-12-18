using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public interface IAudioService : IService
    {
        void Init();

        public void PlaySound(string audio, float volume = 1f, float pitch = 1, bool loop = false);

        public void PlayMusic(string audio, float volume = 1f, float pitch = 1, bool loop = false);

        public void StopAudio(string audio);

        public void StopAudio(AudioSource source);
    }
}
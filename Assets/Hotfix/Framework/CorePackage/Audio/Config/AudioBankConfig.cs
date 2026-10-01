using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    [Serializable]
    [CreateAssetMenu(menuName = "KaAudio/Audio Bank", fileName = "New Audio Bank.kab")]
    public class AudioBankConfig : AudioConfigBase
    {
        public string Name;
        public List<AudioEventConfig> AudioEvents;
    }
}
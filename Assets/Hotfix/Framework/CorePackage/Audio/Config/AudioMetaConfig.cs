using System;
using System.Collections.Generic;

namespace LccHotfix
{
    [Serializable]
    public class AudioMetaConfig
    {
        public static readonly string Path = "AudioMeta";
        
        public List<string> InitBanks;
    }
}
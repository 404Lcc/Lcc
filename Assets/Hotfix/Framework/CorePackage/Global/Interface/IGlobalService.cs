using UnityEngine;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IGlobalService : IService
    {
        Transform Global { get; set; }
        AudioSource Music { get; set; }
        AudioSource SoundFX { get; set; }
        VideoPlayer VideoPlayer { get; set; }
    }
}
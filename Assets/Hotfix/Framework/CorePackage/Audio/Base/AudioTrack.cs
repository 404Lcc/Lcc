
using UnityEngine.Audio;

namespace LccHotfix
{
    /// <summary>
    /// AudioTrack 对应混音器的一个轨道
    /// 控制音量或者调整效果器时需要
    /// </summary>
    public class AudioTrack
    {
        public AudioMixerGroup Mixer { get; private set; }

        private string _volumeParamName;
        
        private float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_volume > 1.0f)
                {
                    _volume = 1.0f;
                }
                else if (_volume < 0.0f)
                {
                    _volume = 0.0f;
                }
                if (Mixer == null)
                {
                    return;
                }
                if (Muted)
                {
                    return;
                }
                Mixer.audioMixer.SetFloat(_volumeParamName, VolumeUtil.VolumeToDB(_volume));
            }
        }
        
        private bool _muted;
        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                Mixer.audioMixer.SetFloat(_volumeParamName,
                    value ? VolumeUtil.MuteDB : VolumeUtil.VolumeToDB(_volume));
            }
        }
        
        public AudioTrack(AudioMixer mixer, string path)
        {
            Mixer = mixer.FindMatchingGroups(path)[0];
            if (Mixer == null)
            {
                Audio.LogError($"[Audio] load mixer group '{path}' failed");
                return;
            }
            _volumeParamName = path + "-Volume";
        }

        public void SetMute(bool muted)
        {
            Muted = muted;
        }

        public void SetVolume(float volume)
        {
            Volume = volume;
        }
    }
}
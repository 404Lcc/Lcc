using ET;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class AudioManager : Singleton<AudioManager>
    {
        public Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();
        public bool AudioExist(string audio)
        {
            if (audioDict.ContainsKey(audio))
            {
                return true;
            }
            return false;
        }
        public AudioClip LoadAudio(string audio)
        {
            AudioClip clip = AssetManager.Instance.LoadAsset<AudioClip>(audio, AssetSuffix.Mp3, AssetType.Audio);
            audioDict.Add(audio, clip);
            return clip;
        }
        public async ETTask<AudioClip> LoadAudio(string audio, AudioType type)
        {
            return await WebUtil.DownloadAudioClip(audio, type);
        }
        public void RemoveAudio(string audio, AudioSource source)
        {
            if (AudioExist(audio))
            {
                source.clip = null;
                source.Stop();
                audioDict.Remove(audio);
            }
        }
        public async ETTask<AudioClip> PlayAudio(string audio, bool isAsset, AudioSource source)
        {
            AudioClip clip;
            if (AudioExist(audio))
            {
                clip = GetAudioClip(audio);
                source.clip = clip;
                source.Play();
                return clip;
            }
            if (isAsset)
            {
                clip = LoadAudio(audio);
            }
            else
            {
                clip = await LoadAudio(audio, AudioType.WAV);
                if (clip == null)
                {
                    return null;
                }
                audioDict.Add(audio, clip);
            }
            source.clip = clip;
            source.Play();
            return clip;
        }
        public void PauseAudio(AudioSource source)
        {
            if (source.isPlaying)
            {
                source.Pause();
            }
        }
        public void SetVolume(float value, AudioSource source)
        {
            source.volume = value;
        }
        public AudioClip GetAudioClip(string audio)
        {
            if (AudioExist(audio))
            {
                AudioClip clip = audioDict[audio];
                return clip;
            }
            return null;
        }
        public bool IsPlayAudio(string audio, AudioSource source)
        {
            if (AudioExist(audio))
            {
                AudioClip clip = GetAudioClip(audio);
                if (source.clip == clip && source.isPlaying)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
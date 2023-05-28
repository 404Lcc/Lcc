using ET;
using LccModel;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class AudioManager : AObjectBase
    {
        public static AudioManager Instance { get; set; }

        public Dictionary<string, AssetOperationHandle> audioDict = new Dictionary<string, AssetOperationHandle>();

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in audioDict.Values)
            {
                AssetManager.Instance.UnLoadAsset(item);
            }
            audioDict.Clear();
            Instance = null;
        }

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
            AssetManager.Instance.LoadAsset<AudioClip>(out AssetOperationHandle handle, audio, AssetSuffix.Mp3, AssetType.Audio);
            audioDict.Add(audio, handle);
            return (AudioClip)handle.AssetObject;
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
                AssetOperationHandle loadHandle = audioDict[audio];
                AudioClip clip = (AudioClip)loadHandle.AssetObject;
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
using LccModel;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class AudioManager : AObjectBase
    {
        public static AudioManager Instance { get; set; }

        public Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();
        public GameObject loader;
        public override void Awake()
        {
            base.Awake();

            Instance = this;
            loader = new GameObject("loader");
            GameObject.DontDestroyOnLoad(loader);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            audioDict.Clear();
            Instance = null;

            GameObject.Destroy(loader);
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
            var clip = AssetManager.Instance.LoadRes<AudioClip>(loader, audio);
            audioDict.Add(audio, clip);
            return clip;
        }
        public AudioClip LoadAudio(string audio, AudioType type)
        {
            return null;//WebHelper.DownloadAudioClip(audio, type);
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
        public AudioClip PlayAudio(string audio, bool isAsset, AudioSource source)
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
                clip = LoadAudio(audio, AudioType.WAV);
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
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace LccHotfix
{
    public class AudioManager : Singleton<AudioManager>
    {
        public Hashtable audios = new Hashtable();
        private bool AudioExist(string audio)
        {
            if (audios.ContainsKey(audio))
            {
                return true;
            }
            return false;
        }
        public async Task<AudioClip> LoadAudio(string audio)
        {
            AudioClip clip = await LccModel.AssetManager.Instance.LoadAsset<AudioClip>(audio, ".mp3", false, true, AssetType.Audio);
            audios.Add(audio, clip);
            return clip;
        }
        public void LoadAudio(string audio, AudioType type, Action<AudioClip> action)
        {
            StartCoroutine(WebUtil.Download(audio, type, action));
        }
        public void RemoveAudio(string audio, AudioSource source)
        {
            if (AudioExist(audio))
            {
                source.clip = null;
                source.Stop();
                audios.Remove(audio);
            }
        }
        public async Task<AudioClip> PlayAudio(string audio, bool isInside, AudioSource source)
        {
            if (AudioExist(audio))
            {
                AudioClip clip = GetAudioClip(audio);
                source.clip = clip;
                source.Play();
                return clip;
            }
            if (isInside)
            {
                AudioClip temp = await LoadAudio(audio);
                source.clip = temp;
                source.Play();
                return temp;
            }
            else
            {
                LoadAudio(audio, AudioType.WAV, (AudioClip clip) =>
                {
                    audios.Add(audio, clip);
                    source.clip = clip;
                    source.Play();
                });
                return null;
            }
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
                AudioClip clip = audios[audio] as AudioClip;
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
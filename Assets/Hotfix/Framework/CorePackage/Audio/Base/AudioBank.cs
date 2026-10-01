
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// AudioBank 是一坨音频素材的集合，可以被整体的加载和卸载
    /// 不需要使用的AudioBank应当及时从内存中卸载
    /// </summary>
    public class AudioBank
    {
        public AudioBankConfig Config { get; private set; }

        private Dictionary<string, AudioEventConfig> _audioEventDict = new();
        private Dictionary<string, AudioAsset<AudioClip>> _audioClipAssets = new();
        public bool IsLoaded { get; private set; }

        public static AudioBank Create(string path)
        {
            return new AudioBank()
            {
                Config = AudioRawFile<AudioBankConfig>.Load(path),
            };
        }
        
        public static AudioBank Load(string path)
        {
            var bank = Create(path);
            bank.LoadSync();
            return bank;
        }
        
        public void LoadSync()
        {
            if (IsLoaded)
            {
                return;
            }
            foreach (var audioEventConfig in Config.AudioEvents)
            {
                foreach (var path in audioEventConfig.Clips)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }
                    if (_audioClipAssets.ContainsKey(path))
                    {
                        continue;
                    }
                    var audioAsset = AudioAsset<AudioClip>.Load(path);
                    if (audioAsset.Asset is null)
                    {
                        continue;
                    }
                    _audioClipAssets.Add(path, audioAsset);
                }
                _audioEventDict.TryAdd(audioEventConfig.Name, audioEventConfig);
            }
            IsLoaded = true;
        }

        public IEnumerator LoadAsync(Action callback = null)
        {
            if (!IsLoaded)
            {
                foreach (var audioEventConfig in Config.AudioEvents)
                {
                    foreach (var path in audioEventConfig.Clips)
                    {
                        if (_audioClipAssets.ContainsKey(path))
                        {
                            continue;
                        }
                        var audioAsset = new AudioAsset<AudioClip>();
                        yield return audioAsset.LoadASync(path);
                        _audioClipAssets.Add(path, audioAsset);
                    }
                    _audioEventDict.TryAdd(audioEventConfig.Name, audioEventConfig);
                }
            }
            IsLoaded = true;
            callback?.Invoke();
        }
        
        public void Unload()
        {
            foreach (var audioAsset in _audioClipAssets.Values)
            {
                audioAsset.Unload();
            }
            _audioClipAssets.Clear();
            _audioEventDict.Clear();
            IsLoaded = false;
        }

        public AudioEventConfig GetEventConfig(string eventName)
        {
            return _audioEventDict.TryGetValue(eventName, out var config) ? config : null;
        }

        public AudioClip GetClip(string clipName)
        {
            return _audioClipAssets.TryGetValue(clipName, out var audioAsset) ? audioAsset.Asset : null;
        }
    }
    
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class AudioSaveData : ISave
    {
        public string TypeName => GetType().FullName;
        public float SoundVolume { get; set; } = 1;
        public float MusicVolume { get; set; } = 1;

        public void Init()
        {
        }
    }

    public class AudioSourcePool : IDisposable
    {
        private GameObject _root;
        private Queue<AudioSource> _pool = new Queue<AudioSource>();
        private List<AudioSource> _activeSourceList = new List<AudioSource>();

        public AudioSourcePool(int initialSize)
        {
            _root = new GameObject("AudioRoot");
            GameObject.DontDestroyOnLoad(_root);
            ExpandPool(initialSize);
        }

        private void ExpandPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject audioSource = new GameObject("AudioSource", typeof(AudioSource));
                audioSource.transform.SetParent(_root.transform);
                _pool.Enqueue(audioSource.GetComponent<AudioSource>());
            }
        }

        public AudioSource Get()
        {
            if (_pool.Count == 0)
            {
                ExpandPool(3);
            }

            var source = _pool.Dequeue();
            _activeSourceList.Add(source);
            return source;
        }

        public void Return(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.volume = 1f; // 重置为默认值
            source.loop = false;

            _pool.Enqueue(source);
            _activeSourceList.Remove(source);
        }

        public List<AudioSource> GetActiveSources()
        {
            return _activeSourceList;
        }

        public void Dispose()
        {
            foreach (var item in _activeSourceList)
            {
                Return(item);
            }
            _activeSourceList.Clear();
            foreach (var item in _pool)
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
            _pool.Clear();
            GameObject.DestroyImmediate(_root.gameObject);
        }
    }

    internal class AudioManager : Module, IAudioService
    {
        private AudioSourcePool _audioPool;

        private AssetLoader _assetLoader;
        private AudioSaveData _save;

        public float SoundVolume
        {
            get => _save.SoundVolume;
            set
            {
                _save.SoundVolume = value;
            }
        }

        public float MusicVolume
        {
            get => _save.MusicVolume;
            set { _save.MusicVolume = value; }
        }

        public AudioManager()
        {
            _audioPool = new AudioSourcePool(10);
            _assetLoader = new AssetLoader();
        }
        
        public void Init()
        {
            _save = Main.SaveService.GetGlobalGameSaveFileSave<AudioSaveData>();
        }
        
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            _audioPool.Dispose();
            _assetLoader.Release();
            _save = null;
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        private void LoadAudio(string audio, Action<AssetHandle> onCompleted)
        {
            _assetLoader.LoadAssetAsync<AudioClip>(audio, onCompleted);
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public void PlaySound(string audio, float volume = 1f, float pitch = 1, bool loop = false)
        {
            LoadAudio(audio, (x) =>
            {
                var source = _audioPool.Get();
                source.clip = x.GetAssetObject<AudioClip>();
                source.volume = volume * _save.SoundVolume;
                source.pitch = pitch;
                source.loop = loop;
                source.Play();
            });
        }

        public void PlayMusic(string audio, float volume = 1f, float pitch = 1, bool loop = false)
        {
            LoadAudio(audio, (x) =>
            {
                var source = _audioPool.Get();
                source.clip = x.GetAssetObject<AudioClip>();
                source.volume = volume * _save.MusicVolume;
                source.pitch = pitch;
                source.loop = loop;
                source.Play();
            });
        }

        /// <summary>
        /// 停止接口
        /// </summary>
        public void StopAudio(string audio)
        {
            var list = _audioPool.GetActiveSources().Where(s => s.clip != null && s.clip.name == audio).ToList();
            foreach (var item in list)
            {
                StopAudio(item);
            }
        }

        public void StopAudio(AudioSource source)
        {
            if (source == null)
                return;

            _audioPool.Return(source);
        }
    }
}
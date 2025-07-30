using System;
using LccModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public class AudioSaveData : ISave
    {
        public float SoundVolume { get; set; } = 1;
        public float MusicVolume { get; set; } = 1;

        public void Init()
        {
        }
    }

    public class AudioData : ISaveConverter<AudioSaveData>
    {
        public AudioSaveData Save { get; set; }
        public float SoundVolume { get; set; }
        public float MusicVolume { get; set; }

        public void Flush()
        {
            Save.SoundVolume = SoundVolume;
            Save.MusicVolume = MusicVolume;
        }

        public void Init()
        {
            this.SoundVolume = Save.SoundVolume;
            this.MusicVolume = Save.MusicVolume;
        }
    }

    public class AudioAsset
    {
        public ResObject ResObject { get; set; }
        public int RefCount { get; set; }
        public DateTime LastUsedTime { get; set; }

        public AudioClip GetClip()
        {
            if (ResObject == null)
                return null;
            return ResObject.GetAsset<AudioClip>();
        }

        public void Release()
        {
            if (ResObject != null)
            {
                ResObject.Release();
                ResObject = null;
            }
        }
    }

    public class AudioSourcePool : IDisposable
    {
        private Queue<AudioSource> _pool = new Queue<AudioSource>();
        private List<AudioSource> _activeSourceList = new List<AudioSource>();

        public AudioSourcePool(int initialSize)
        {
            ExpandPool(initialSize);
        }

        private void ExpandPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Main.GameObjectPoolService.GetObject("AudioSource").GameObject;
                var source = obj.GetComponent<AudioSource>();
                _pool.Enqueue(source);
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
            _pool.Clear();
            _activeSourceList.Clear();
            Main.GameObjectPoolService.ReleasePool("AudioSource");
        }
    }

    internal class AudioManager : Module, IAudioService
    {
        private const int MaxCache = 20;
        private const int CacheCleanThreshold = 15;

        private Dictionary<string, AudioAsset> _audioAssets = new Dictionary<string, AudioAsset>();
        private GameObject loader;

        private AudioSourcePool _audioPool;

        private HashSet<AudioSource> _activeSources = new HashSet<AudioSource>();
        private List<AudioSource> _removeList = new List<AudioSource>(10);

        private AudioData save;

        public float SoundVolume
        {
            get => save.SoundVolume;
            set { save.SoundVolume = value; }
        }

        public float MusicVolume
        {
            get => save.MusicVolume;
            set { save.MusicVolume = value; }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 每帧检测临时音源状态
            if (_activeSources.Count > 0)
            {
                _removeList.Clear();

                foreach (var source in _activeSources)
                {
                    if (!source.isPlaying || source.clip == null)
                    {
                        _removeList.Add(source);
                        _audioPool.Return(source);

                        if (source.clip != null && _audioAssets.TryGetValue(source.clip.name, out var asset))
                        {
                            asset.RefCount = Math.Max(0, asset.RefCount - 1);
                        }
                    }
                }

                foreach (var removeSource in _removeList)
                {
                    _activeSources.Remove(removeSource);
                }
            }

            AutoReleaseAsset();
        }

        internal override void Shutdown()
        {
            foreach (var item in _audioAssets.Values)
            {
                item.Release();
            }

            _audioAssets.Clear();
            _audioPool.Dispose();

            GameObject.Destroy(loader);
        }


        public AudioManager()
        {
            _audioPool = new AudioSourcePool(10);
            loader = new GameObject("loader");
            GameObject.DontDestroyOnLoad(loader);

            save = Main.SaveService.GetSaveData<AudioData, AudioSaveData>();
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        private AudioClip LoadAudio(string audio)
        {
            if (_audioAssets.TryGetValue(audio, out var asset))
            {
                asset.RefCount++;
                asset.LastUsedTime = DateTime.Now;
                return asset.GetClip();
            }

            var resObject = ResObject.LoadRes<AudioClip>(loader, audio);
            if (resObject == null)
            {
                Debug.LogError($"Failed to load audio: {audio}");
                return null;
            }

            var newAsset = new AudioAsset();
            newAsset.ResObject = resObject;
            newAsset.RefCount = 1;
            newAsset.LastUsedTime = DateTime.Now;
            _audioAssets.Add(audio, newAsset);
            return newAsset.GetClip();
        }


        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public AudioSource PlaySound(string audio, float volume = 1f, float pitch = 1, bool loop = false)
        {
            var clip = LoadAudio(audio);
            var source = _audioPool.Get();
            source.clip = clip;
            source.volume = volume * save.SoundVolume;
            source.pitch = pitch;
            source.loop = loop;
            source.Play();

            if (!loop)
            {
                _activeSources.Add(source);
            }

            return source;
        }

        public AudioSource PlayMusic(string audio, float volume = 1f, float pitch = 1, bool loop = false)
        {
            var clip = LoadAudio(audio);
            var source = _audioPool.Get();
            source.clip = clip;
            source.volume = volume * save.MusicVolume;
            source.pitch = pitch;
            source.loop = loop;
            source.Play();

            if (!loop)
            {
                _activeSources.Add(source);
            }

            return source;
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

        private void StopAudio(AudioSource source)
        {
            if (source == null)
                return;

            if (_activeSources.Contains(source))
            {
                _activeSources.Remove(source);
            }

            _audioPool.Return(source);

            if (source.clip != null && _audioAssets.TryGetValue(source.clip.name, out var asset))
            {
                asset.RefCount = Math.Max(0, asset.RefCount - 1);
            }
        }

        /// <summary>
        /// 暂停所有音频
        /// </summary>
        public void PauseAll()
        {
            foreach (var source in _audioPool.GetActiveSources())
            {
                source.Pause();
            }
        }

        /// <summary>
        /// 自动释放资源
        /// </summary>
        private void AutoReleaseAsset()
        {
            int toRemoveCount = _audioAssets.Count - MaxCache;
            if (toRemoveCount > 0)
            {
                var list = _audioAssets.Where(x => x.Value.RefCount == 0).OrderBy(x => x.Value.LastUsedTime).ToList();
                var removeList = list.Take(toRemoveCount).ToList();
                foreach (var item in removeList)
                {
                    item.Value.Release();
                    _audioAssets.Remove(item.Key);
                }
            }
        }
    }
}
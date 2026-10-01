using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LccHotfix
{
    public partial class AudioManager
    {
        private readonly Dictionary<int, AudioEvent> _audioEvents = new();
        private readonly Dictionary<string, AudioEventGroup> _audioEventGroups = new(); 
        private readonly List<AudioEvent> _updateEventCache = new(64); // UpdateEvent 的复用快照，避免每帧 ToList 分配
        private int _audioEventId = 0;

        public int NewEventId()
        {
            var id = Interlocked.Increment(ref _audioEventId);
            while (_audioEvents.ContainsKey(id))
            {
                id = Interlocked.Increment(ref _audioEventId);
            }
            return id;
        }

        public AudioEvent CreateEvent(string bankName, string eventName)
        {
            var bank = GetBank(bankName);
            if (bank is null)
            {
                return null;
            }
            var eventConfig = bank.GetEventConfig(eventName);
            if (eventConfig is null)
            {
                return null;
            }
            var eventType = GetEventType(eventConfig.Event);
            if (eventType is null)
            {
                return null;
            }
            if (!IncEventGroupCounter(eventConfig))
            {
                return null;
            }
            var audioEvent = AudioEventPool.CreateEvent(eventType);
            if (audioEvent is null)
            {
                DecEventGroupCounter(eventConfig);
                return null;
            }
            audioEvent.Init(this, bank, eventConfig);
            _audioEvents.Add(audioEvent.Id, audioEvent);
            audioEvent.Awake();
            return audioEvent;
        }
        
# if UNITY_EDITOR
        public AudioEvent CreateEvent(AudioEventConfig config)
        {
            var eventType = GetEventType(config.Event);
            if (eventType is null)
            {
                return null;
            }
            var audioEvent = AudioEventPool.CreateEvent(eventType);
            if (audioEvent is null)
            {
                return null;
            }
            audioEvent.Init(this, null, config);
            _audioEvents.Add(audioEvent.Id, audioEvent);
            audioEvent.Awake();
            return audioEvent;
        }
#endif

        public List<AudioEvent> GetEvents(string eventType)
        {
            return _audioEvents.Values.Where(audioEvent => audioEvent.Config.Event == eventType).ToList();
        }

        public AudioEvent GetEventById(int eventId)
        {
            return _audioEvents.TryGetValue(eventId, out var audioEvent) ? audioEvent : null;
        }

        private void UpdateEvent()
        {
            // 复用快照：DestroyEvent 会在遍历过程中修改 _audioEvents，不能直接遍历字典
            foreach (var kv in _audioEvents)
            {
                _updateEventCache.Add(kv.Value);
            }

            for (int i = 0; i < _updateEventCache.Count; i++)
            {
                var audioEvent = _updateEventCache[i];
                try
                {
                    if (audioEvent is null)
                    {
                        continue;
                    }
                    audioEvent.Update();
                    if (!audioEvent.IsActive)
                    {
                        DestroyEvent(audioEvent);
                    }
                }
                catch (Exception e)
                {
                    Audio.LogError(e);
                    DestroyEvent(audioEvent);
                }
            }
            _updateEventCache.Clear(); // 断开对已回收实例的引用

            foreach (var kv in _audioEventGroups)
            {
                kv.Value.Update();
            }
        }

        public void DestroyEvent(AudioEvent audioEvent)
        {
            if (audioEvent is null)
            {
                return;
            }
            if (!_audioEvents.ContainsKey(audioEvent.Id))
            {
                return;
            }
            DecEventGroupCounter(audioEvent.Config);
            _audioEvents.Remove(audioEvent.Id);
            audioEvent.Destroy();
            AudioEventPool.ReleaseEvent(audioEvent);
        }

        public void DestroyEvent(int eventId)
        {
            GetEventById(eventId)?.Destroy();
        }

        public void DestroyAllEvent()
        {
            foreach (var audioEvent in _audioEvents.Values.ToList())
            {
                DestroyEvent(audioEvent);
            }
        }

        private void DisposeEvent()
        {
            var audioEvents = _audioEvents.Values.ToList();
            foreach (var audioEvent in audioEvents)
            {
                DestroyEvent(audioEvent);
            }
        }

        private bool IncEventGroupCounter(AudioEventConfig cfg)
        {
            AudioEventGroup selfGroup = null;
            AudioEventGroup tagGroup = null;
            if (!_audioEventGroups.TryGetValue(cfg.Name, out selfGroup))
            {
                selfGroup = Audio.PoolAllocate<AudioEventGroup>();
            }
            if (!string.IsNullOrEmpty(cfg.TagName))
            {
                if (!_audioEventGroups.TryGetValue(cfg.TagName, out tagGroup))
                {
                    tagGroup = Audio.PoolAllocate<AudioEventGroup>();
                }
            }

            if (cfg.MaxStack > 0 && selfGroup.EventCount >= cfg.MaxStack)
            {
                return false;
            }
            if (tagGroup != null && cfg.MaxStack > 0 && tagGroup.EventCount >= cfg.MaxStack && selfGroup.EventCount > 0)
            {
                return false;
            }
            if (selfGroup.LastEventTime >= 0 && selfGroup.LastEventTime < cfg.MinInterval)
            {
                return false;
            }

            selfGroup.EventCount++;
            selfGroup.LastEventTime = 0;
            _audioEventGroups[cfg.Name] = selfGroup;
            if (tagGroup != null)
            {
                tagGroup.EventCount++;
                tagGroup.LastEventTime = 0;
                _audioEventGroups[cfg.TagName] = tagGroup;
            }
            return true;
        }

        private void DecEventGroupCounter(AudioEventConfig cfg)
        {
            if (cfg is null)
            {
                return;
            }

            AudioEventGroup selfGroup = null;
            AudioEventGroup tagGroup = null;
            _audioEventGroups.TryGetValue(cfg.Name, out selfGroup);
            if (!string.IsNullOrEmpty(cfg.TagName))
            {
                _audioEventGroups.TryGetValue(cfg.TagName, out tagGroup);
            }

            if (selfGroup != null)
            {
                selfGroup.EventCount--;
                if (selfGroup.EventCount > 0)
                {
                    _audioEventGroups[cfg.Name] = selfGroup;
                }
                else
                {
                    _audioEventGroups.Remove(cfg.Name);
                    Audio.PoolRecycle(selfGroup);
                }
            }


            if (tagGroup != null)
            {
                tagGroup.EventCount--;
                if (tagGroup.EventCount > 0)
                {
                    _audioEventGroups[cfg.TagName] = tagGroup;
                }
                else
                {
                    _audioEventGroups.Remove(cfg.TagName);
                    Audio.PoolRecycle(tagGroup);
                }
            }
        }

    }
}
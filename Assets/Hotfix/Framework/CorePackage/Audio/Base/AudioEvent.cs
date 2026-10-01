using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LccHotfix
{
    /// <summary>
    /// AudioEvent 是音频的表象层，作用类似FMOD中的一个Event
    /// 对于单次播放音频的一切复杂控制，都要由AudioEvent继承
    /// </summary>
    public class AudioEvent : IReference
    {
        public int Id { get; private set; }
        public AudioEventConfig Config { get; private set; }
        
        protected AudioManager _manager;
        protected AudioBank _bank;
        
        public Vector3 Pos = Vector3.zero;

        // private readonly Dictionary<AudioEventParamType, TimeTransitionFloat> mParams = new Dictionary<AudioEventParamType, TimeTransitionFloat>();
        private readonly List<AudioController> _controllers = new List<AudioController>();
        
        private bool _destroyed = false;
        public bool IsActive => !_destroyed;

        #region 底层数据

        public void Init(AudioManager manager, AudioBank bank, AudioEventConfig config)
        {
            _manager = manager;
            _bank = bank;
            Config = config;
            Id = manager.NewEventId();
            _destroyed = false;
        }

        protected AudioController GetTrackController(string trackName)
        {
            if (_destroyed)
            {
                Audio.LogError("[Audio] event already destroyed");
                return null;
            }
            var track = _manager.GetTrack(trackName);
            var controller = _manager.GetController(this, track);
            if (controller is not null)
            {
                _controllers.Add(controller);
            }
            return controller;
        }
        
        protected AudioController GetController(string clipName, string trackName = null)
        {
            AudioClip clip = null;
            if (_bank is null)
            {
            #if UNITY_EDITOR
                var clips = AssetDatabase.FindAssets(clipName);
                if (clips.Length > 0)
                {
                    clip =  AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(clips[0]));
                }
            #endif
            }
            else
            {
                clip = _bank.GetClip(clipName);
            }
            if (clip is null)
            {
                Destroy();
                return null;
            }
            var controller = GetTrackController(trackName);
            if (controller is null)
            {
                Destroy();
                return null;
            }
            controller.SetClip(clip);
            return controller;
        }

        protected void RecycleController(AudioController controller)
        {
            controller.Stop();
            _controllers.Remove(controller);
        }

        #endregion
        
        #region 生命周期
        
        public virtual void Awake()
        {
            
        }

        public virtual void Play()
        {
            
        }

        public virtual void Update()
        {
            // foreach (var param in mParams.Values)
            // {
            //     param.Update();
            // }
        }

        public virtual void Stop()
        {
            foreach (var controller in _controllers)
            {
                controller.Stop();
            }
        }

        public virtual void Destroy()
        {
            if (_destroyed)
            {
                return;
            }
            _destroyed = true;

            foreach (var controller in _controllers)
            {
                controller.Recycle();
            }
            _controllers.Clear();
        }

        public virtual void OnRecycle()
        {
            Destroy();

            _manager = null;
            _bank = null;
            Config = null;
        }

        #endregion

        #region 运算符重载

        public static bool operator ==(AudioEvent a, AudioEvent b)
        {
            if ((a is null || a._destroyed) && (b is null || b._destroyed))
            {
                return true;
            }
            return a?.Id == b?.Id;
        }
        
        public static bool operator !=(AudioEvent a, AudioEvent b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is AudioEvent @event && Id == @event.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        #endregion
    }
}


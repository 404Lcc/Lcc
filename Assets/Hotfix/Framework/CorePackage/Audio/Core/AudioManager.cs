using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public partial class AudioManager : Module, IAudioService
    {
        public bool IsEditor { get; private set; } // 是否是编辑器模式下运行
        public static GameObject AudioRoot;

        #region 生命周期

        public void Init()
        {
            Register(false);
        }
        
        public void Register(bool isEditor = false)
        {
            IsEditor = isEditor;
            if (IsEditor)
            {
                AudioRoot = new GameObject("TempAudioRoot");
                AudioRoot.transform.SetParent(null);
                AudioRoot.hideFlags = HideFlags.DontSave | HideFlags.DontUnloadUnusedAsset;
                InitAttr();
                InitController();
                return;
            }
            AudioRoot = new GameObject("AudioRoot");
            Object.DontDestroyOnLoad(AudioRoot);
            InitMeta();
            InitAttr();
            InitController();
            InitBank();
            InitTrack();
        }

        public void Update()
        {
            UpdateController();
            UpdateEvent();
        }

        public void Dispose()
        {
            DisposeEvent();
            DisposeController();
            DisposeBank();
            DisposeMeta();
            DisposeTrack();
            if (AudioRoot != null)
            {
                Object.DestroyImmediate(AudioRoot);
                AudioRoot = null;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            Update();
        }

        internal override void Shutdown()
        {
            Dispose();
        }

        #endregion

        #region 便捷接口

        /// <summary>
        /// 播放特定音频事件
        /// </summary>
        /// <param name="bankName">音频库名</param>
        /// <param name="eventName">音频事件名</param>
        public void Play(string bankName, string eventName)
        {
            CreateEvent(bankName, eventName)?.Play();
        }

        /// <summary>
        /// 播放特定音频事件
        /// </summary>
        /// <param name="bankName">音频库名</param>
        /// <param name="eventName">音频事件名</param>
        public void Play(string eventName, Vector3 pos)
        {
            var audioEvent = CreateEvent("Combat", eventName);
            if (audioEvent != null)
            {
                audioEvent.Pos = pos;
                audioEvent.Play();
            }
        }

        /// <summary>
        /// 游戏暂停
        /// </summary>
        public void Pause()
        {
            foreach (var controller in _audioControllers.
                         Where(controller =>
                            controller.AudioEvent != null &&
                            controller.AudioEvent.Config != null &&
                            controller.AudioEvent.Config.CanPause &&
                            controller.IsPlaying))
            {
                controller.Pause();
            }
        }

        /// <summary>
        /// 游戏暂停恢复
        /// </summary>
        public void Resume()
        {
            foreach (var controller in _audioControllers.
                         Where(controller => controller.IsPaused))
            {
                controller.Resume();
            }
        }

        /// <summary>
        /// 清理音频
        /// </summary>
        public void Clean()
        {
            var audioEvents = _audioEvents.Values.ToList();
            foreach (var audioEvent in audioEvents.
                         Where(audioEvent => audioEvent.Config.CanClean))
            {
                DestroyEvent(audioEvent);
            }
            //MixerToSnapshot("Normal", 0);
        }

        #endregion
    }
}
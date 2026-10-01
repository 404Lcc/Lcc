using UnityEngine;

namespace LccHotfix
{
    [AudioEventAttr]
    public class AudioEventUI : AudioEvent
    {
        protected AudioController _controller;
        
        public override void Awake()
        {
        }

        public override void Play()
        {
            if (Config.Clips.Count == 0)
            {
                return;
            }
            _controller = GetController(Config.Clips[0], "Master/UI");
            if (_controller is null)
            {
                return;
            }
            _controller.Pitch = 1f + (Random.value * 2f - 1f) * Config.PitchShift;
            _controller.Play(false, Config.Volume);
        }

        public override void Update()
        {
            base.Update();
            if (_controller is not {IsActive: true})
            {
                Destroy();
            }
        }
    }
}
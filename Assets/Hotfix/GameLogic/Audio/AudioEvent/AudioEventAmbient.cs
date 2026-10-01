using UnityEngine;

namespace LccHotfix
{
    [AudioEventAttr]
    public class AudioEventAmbient : AudioEvent
    {
        protected AudioController _controller;
        private TimeTransitionFloat _fadeOutVolume = TimeTransitionFloat.NewVolume(1f, 5f);

        public override void Play()
        {
            if (Config.Clips.Count == 0)
            {
                return;
            }
            _controller = GetController(Config.Clips[Random.Range(0, Config.Clips.Count)], "Master/Ambient");
            if (_controller is null)
            {
                return;
            }
            _controller.Pitch = 1f + (Random.value * 2f - 1f) * Config.PitchShift;
            _fadeOutVolume.ForceSet(1f);
            _controller.Play(true, Config.Volume * _fadeOutVolume, 0);
        }

        public override void Update()
        {
            base.Update();
            if (_controller is not { IsActive: true } || _controller.Volume <= VolumeUtil.MinVolume)
            {
                Destroy();
            }
            else
            {
                _fadeOutVolume.Update();
            }
        }

        public override void Stop()
        {
            _fadeOutVolume.TargetValue = 0f;
        }
    }
}
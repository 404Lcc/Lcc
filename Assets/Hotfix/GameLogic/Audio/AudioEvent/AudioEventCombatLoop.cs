
namespace LccHotfix
{
    [AudioEventAttr]
    public class AudioEventCombatLoop : AudioEventCombat
    {
        private TimeTransitionFloat _fadeOutVolume = TimeTransitionFloat.NewVolume(0f, 5f);
        public override void Play()
        {
            base.Play();
            _fadeOutVolume.ForceSet(VolumeUtil.MinVolume);
            _fadeOutVolume.TargetValue = 1f;
            _controller.SetLoop(true);
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
                var stereoVolume = _manager.Stereo.Volume(Pos);
                _controller.Volume = Config.Volume * stereoVolume * _fadeOutVolume;
                _controller.Panning = _manager.Stereo.Pan(Pos);
            }
        }

        public override void Stop()
        {
            _fadeOutVolume.TargetValue = 0f;
        }
    }
}

using UnityEngine;

namespace LccHotfix
{
    [AudioEventAttr]
    public class AudioEventCombat : AudioEvent
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
            _controller = GetController(Config.Clips[Random.Range(0, Config.Clips.Count)], "Master/Combat");
            if (_controller is null)
            {
                return;
            }
            _controller.Pitch = 1f + (Random.value * 2f - 1f) * Config.PitchShift;
            _controller.Play(false, Config.Volume * _manager.Stereo.Volume(Pos), _manager.Stereo.Pan(Pos));
        }

        public override void Update()
        {
            base.Update();
            if (_controller is not {IsActive: true})
            {
                Destroy();
            }
            else
            {
                _controller.Volume = Config.Volume * _manager.Stereo.Volume(Pos);
                _controller.Panning = _manager.Stereo.Pan(Pos);
            }
        }
    }
}
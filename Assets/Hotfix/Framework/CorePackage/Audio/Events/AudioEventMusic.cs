using System.Linq;

namespace LccHotfix
{
    [AudioEventAttr]
    public class AudioEventMusic : AudioEvent
    {
        protected AudioController _controller;
        
        public override void Awake()
        {
            Config.CanPause = false;
            Config.CanClean = false;
        }

        public override void Play()
        {
            var allMusicEvents = _manager.GetEvents(GetType().Name);
            foreach (var otherMusicEvent in allMusicEvents.Where(musicEvent => musicEvent != this))
            {
                otherMusicEvent.Stop();
            }
            if (Config.Clips.Count == 0)
            {
                return;
            }
            _controller = GetController(Config.Clips[0], "Master/Music");
            if (_controller is null)
            {
                return;
            }
            _controller.Play(true, Config.Volume);
        }

        public override void Update()
        {
            base.Update();
            if (_controller is not {IsActive: true} || _controller.Volume < float.Epsilon)
            {
                Destroy();
            }
        }
        
        public override void Stop()
        {
            _controller.Volume = 0;
        }
    }
}
using UnityEngine;

namespace LccHotfix
{
    public class AudioStereoBase : IAudioStereo
    {
        protected virtual Vector2 WorldToScreen(Vector3 pos)
        {
            return pos;
        }

        protected virtual float ScreenPan(Vector2 pos)
        {
            return 0.0f;
        }  
        
        protected virtual float ScreenVolume(Vector2 pos)
        {
            return 1.0f;
        }

        public float Volume(Vector3 pos)
        {
            return ScreenVolume(WorldToScreen(pos));
        }

        public float Pan(Vector3 pos)
        {
            return ScreenPan(WorldToScreen(pos));
        }
    }
}
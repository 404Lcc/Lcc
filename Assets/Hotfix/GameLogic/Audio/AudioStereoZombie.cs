using UnityEngine;

namespace LccHotfix
{
    public class AudioStereoZombie
        : AudioStereoBase, IAudioStereo
    {
        protected override Vector2 WorldToScreen(Vector3 pos)
        {
            return Main.CameraService?.CurrentCamera?.WorldToViewportPoint(pos) ?? Vector2.zero;
        }

        protected override float ScreenPan(Vector2 pos)
        {
            return Mathf.Clamp((pos.x - 0.5f) * 1.5f, -1f, 1f);
        }

        protected override float ScreenVolume(Vector2 pos)
        {
            return 1f - Mathf.Clamp(Mathf.Abs(0.5f - pos.y) * 0.5f + Mathf.Abs(0.5f - pos.x) * 0.25f, 0f, 1f);
        }
    }
}
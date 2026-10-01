using System;

namespace LccHotfix
{
    public static class VolumeUtil
    {
        public const float MinVolume = 0.001f; // 最小音量

        public const float MuteDB = -100.0f;
        private static readonly float Log10 = (float)Math.Log(10);
        
        // 线性音量转分贝
        public static float VolumeToDB(float representationVolume)
        {
            if (representationVolume <= 0)
            {
                return MuteDB;
            }

            if (representationVolume >= 1)
            {
                return 0.0f;
            }

            var volume = Math.Log10(representationVolume) * 10.0f;
            if (volume < MuteDB)
            {
                return MuteDB;
            }
            return (float)volume;
        }

        // 分贝转线性音量
        public static float DBToVolume(float dB)
        {
            if (dB >= 0)
            {
                return 1.0f;
            }
            
            if (dB <= MuteDB)
            {
                return 0.0f;
            }
            
            return (float) Math.Exp(dB / 10.0f) * Log10;
        }
    }
}
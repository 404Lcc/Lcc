using System.Runtime.CompilerServices;
using UnityEngine;

namespace LccHotfix
{
    public static class EasingFunctions
    {
        public static float ApplyEasing(float t, EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.Linear: return Linear(t);

                case EasingType.QuadIn: return QuadIn(t);
                case EasingType.QuadOut: return QuadOut(t);
                case EasingType.QuadOutIn: return QuadOutIn(t);

                case EasingType.CubicIn: return CubicIn(t);
                case EasingType.CubicOut: return CubicOut(t);
                case EasingType.CubicInOut: return CubicInOut(t);

                case EasingType.QuartIn: return QuartIn(t);
                case EasingType.QuartOut: return QuartOut(t);
                case EasingType.QuartInOut: return QuartInOut(t);

                case EasingType.QuintIn: return QuintIn(t);
                case EasingType.QuintOut: return QuintOut(t);
                case EasingType.QuintInOut: return QuintInOut(t);

                case EasingType.SineIn: return SineIn(t);
                case EasingType.SineOut: return SineOut(t);
                case EasingType.SineInOut: return SineInOut(t);

                case EasingType.CircIn: return CircIn(t);
                case EasingType.CircOut: return CircOut(t);
                case EasingType.CircInOut: return CircInOut(t);

                case EasingType.ExpoIn: return ExpoIn(t);
                case EasingType.ExpoOut: return ExpoOut(t);
                case EasingType.ExpoInOut: return ExpoInOut(t);

                case EasingType.ElasticIn: return ElasticIn(t);
                case EasingType.ElasticOut: return ElasticOut(t);
                case EasingType.ElasticInOut: return ElasticInOut(t);

                case EasingType.BackIn: return BackIn(t);
                case EasingType.BackOut: return BackOut(t);
                case EasingType.BackInOut: return BackInOut(t);

                case EasingType.BounceIn: return BounceIn(t);
                case EasingType.BounceOut: return BounceOut(t);
                case EasingType.BounceInOut: return BounceInOut(t);
            }

            return t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Linear(float t) => t;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadIn(float t) => t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadOut(float t) => -(t * (t - 2));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadOutIn(float t) => t < 0.5f ? 2 * t * t : (-2 * t * t) + (4 * t) - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicIn(float t) => t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicOut(float t) => (t - 1) * (t - 1) * (t - 1) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicInOut(float t) => t < 0.5f ? 4 * t * t * t : 0.5f * ((2 * t) - 2) * ((2 * t) - 2) * ((2 * t) - 2) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuartIn(float t) => t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuartOut(float t) => (t - 1) * (t - 1) * (t - 1) * (1 - t) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuartInOut(float t) => t < 0.5f ? 8 * t * t * t * t : -8 * (t - 1) * (t - 1) * (t - 1) * (t - 1) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuintIn(float t) => t * t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuintOut(float t) => (t - 1) * (t - 1) * (t - 1) * (t - 1) * (t - 1) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuintInOut(float t) => t < 0.5f ? 16 * t * t * t * t * t : 0.5f * ((2 * t) - 2) * ((2 * t) - 2) * ((2 * t) - 2) * ((2 * t) - 2) * ((2 * t) - 2) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SineIn(float t) => Mathf.Sin((t - 1) * Mathf.PI / 2f) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SineOut(float t) => Mathf.Sin(t * Mathf.PI / 2f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SineInOut(float t) => 0.5f * (1 - Mathf.Cos(t * Mathf.PI));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CircIn(float t) => 1 - Mathf.Sqrt(1 - (t * t));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CircOut(float t) => Mathf.Sqrt((2 - t) * t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CircInOut(float t) => t < 0.5f ? 0.5f * (1 - Mathf.Sqrt(1 - 4 * (t * t))) : 0.5f * (Mathf.Sqrt(-((2 * t) - 3) * ((2 * t) - 1)) + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ExpoIn(float t) => Mathf.Approximately(t, 0.0f) ? t : Mathf.Pow(2, 10 * (t - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ExpoOut(float t) => Mathf.Approximately(t, 1.0f) ? t : 1 - Mathf.Pow(2, -10 * t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ExpoInOut(float t) => (Mathf.Approximately(t, 0.0f) || Mathf.Approximately(t, 1.0f)) ? t : t < 0.5f ? 0.5f * Mathf.Pow(2, (20 * t) - 10) : -0.5f * Mathf.Pow(2, (-20 * t) + 10) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ElasticIn(float t) => Mathf.Sin(13 * Mathf.PI / 2 * t) * Mathf.Pow(2, 10 * (t - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ElasticOut(float t) => Mathf.Sin(-13 * Mathf.PI / 2 * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ElasticInOut(float t) => t < 0.5f ? 0.5f * Mathf.Sin(13 * Mathf.PI / 2 * (2 * t)) * Mathf.Pow(2, 10 * ((2 * t) - 1)) : 0.5f * (Mathf.Sin(-13 * Mathf.PI / 2 * ((2 * t - 1) + 1)) * Mathf.Pow(2, -10 * (2 * t - 1)) + 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BackIn(float t) => t * t * t - t * Mathf.Sin(t * Mathf.PI);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BackOut(float t) => 1 - ((1 - t) * (1 - t) * (1 - t) - (1 - t) * Mathf.Sin((1 - t) * Mathf.PI));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BackInOut(float t) => t < 0.5f ? 0.5f * (2 * t * 2 * t * 2 * t - 2 * t * Mathf.Sin(2 * t * Mathf.PI)) : 0.5f * (1 - ((1 - (2 * t - 1)) * (1 - (2 * t - 1)) * (1 - (2 * t - 1)) - (1 - (2 * t - 1)) * Mathf.Sin((1 - (2 * t - 1)) * Mathf.PI))) + 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BounceIn(float t) => 1 - BounceOut(1 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BounceInOut(float t) => t < 0.5f ? 0.5f * BounceIn(t * 2) : 0.5f * BounceOut(t * 2 - 1) + 0.5f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float BounceOut(float t)
        {
            if (t < 4 / 11.0f)
            {
                return (121 * t * t) / 16.0f;
            }
            else if (t < 8 / 11.0f)
            {
                return (363 / 40.0f * t * t) - (99 / 10.0f * t) + 17 / 5.0f;
            }
            else if (t < 9 / 10.0f)
            {
                return (4356 / 361.0f * t * t) - (35442 / 1805.0f * t) + 16061 / 1805.0f;
            }
            else
            {
                return (54 / 5.0f * t * t) - (513 / 25.0f * t) + 268 / 25.0f;
            }
        }
    }
}
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace LccHotfix
{
    internal class EasingManager : Module, IEasingService
    {
        
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        public IEasingCoroutine DoFloat(float from, float to, float duration, UnityAction<float> action, float delay = 0)
        {
            return new FloatEasingCoroutine(from, to, duration, delay, action);
        }

        public IEasingCoroutine DoAfter(float seconds, UnityAction action, bool unscaledTime = false)
        {
            return new WaitCoroutine(seconds, unscaledTime).SetOnFinish(action);
        }

        public IEasingCoroutine DoAfter(Func<bool> condition)
        {
            return new WaitForConditionCoroutine(condition);
        }

        public IEasingCoroutine DoNextFrame()
        {
            return new NextFrameCoroutine();
        }
        public IEasingCoroutine DoNextFrame(UnityAction action)
        {
            return new NextFrameCoroutine().SetOnFinish(action);
        }

        public IEasingCoroutine DoNextFixedFrame()
        {
            return new NextFixedFrameCoroutine();
        }
    }

    public interface IEasingCoroutine
    {
        bool IsActive { get; }
        IEasingCoroutine SetEasing(EasingType easingType);
        IEasingCoroutine SetEasingCurve(AnimationCurve easingCurve);
        IEasingCoroutine SetOnFinish(UnityAction callback);
        IEasingCoroutine SetUnscaledTime(bool unscaledTime);
        IEasingCoroutine SetDelay(float delay);
        void Stop();
    }

    public abstract class EmptyCoroutine : IEasingCoroutine, ICoroutine
    {
        protected CoroutineHandler coroutine;

        public bool IsActive { get; protected set; }

        protected UnityAction finishCallback;

        protected EasingType easingType = EasingType.Linear;

        protected float delay = -1;

        protected bool unscaledTime;

        protected bool useCurve;

        protected AnimationCurve easingCurve;

        public IEasingCoroutine SetEasing(EasingType easingType)
        {
            this.easingType = easingType;
            useCurve = false;
            return this;
        }

        public IEasingCoroutine SetOnFinish(UnityAction callback)
        {
            finishCallback = callback;
            return this;
        }

        public IEasingCoroutine SetUnscaledTime(bool unscaledTime)
        {
            this.unscaledTime = unscaledTime;
            return this;
        }

        public IEasingCoroutine SetEasingCurve(AnimationCurve curve)
        {
            easingCurve = curve;
            useCurve = true;

            return this;
        }

        public IEasingCoroutine SetDelay(float delay)
        {
            this.delay = delay;

            return this;
        }

        public void Stop()
        {
            coroutine.Stop();

            IsActive = false;
        }
    }

    public class NextFrameCoroutine : EmptyCoroutine
    {
        public NextFrameCoroutine()
        {
            coroutine = this.StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            yield return null;

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class NextFixedFrameCoroutine : EmptyCoroutine
    {
        public NextFixedFrameCoroutine()
        {
            coroutine = this.StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            yield return new WaitForFixedUpdate();

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class WaitCoroutine : EmptyCoroutine
    {
        protected float duration;

        public WaitCoroutine(float duration, bool unscaledTime = false)
        {
            this.duration = duration;
            this.unscaledTime = unscaledTime;

            coroutine = this.StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            if (unscaledTime)
            {
                yield return new WaitForSecondsRealtime(duration);
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class WaitForConditionCoroutine : EmptyCoroutine
    {
        private Func<bool> condition;

        public WaitForConditionCoroutine(Func<bool> condition)
        {
            this.condition = condition;
            coroutine = this.StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            do
            {
                yield return null;
            } while (!condition());

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public abstract class EasingCoroutine<T> : EmptyCoroutine
    {
        protected T from;
        protected T to;
        protected float duration;

        protected UnityAction<T> callback;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T Lerp(T a, T b, float t);

        public EasingCoroutine(T from, T to, float duration, float delay, UnityAction<T> callback)
        {
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.callback = callback;
            this.delay = delay;

            coroutine = this.StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            float time = 0;

            while (time < duration)
            {
                yield return null;

                if (delay > 0)
                {
                    delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                    if (delay > 0) continue;
                }

                time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t;
                if (useCurve)
                {
                    t = easingCurve.Evaluate(time / duration);
                }
                else
                {
                    t = EasingFunctions.ApplyEasing(time / duration, easingType);
                }

                T value = Lerp(from, to, t);
                callback?.Invoke(value);
            }

            callback.Invoke(to);
            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class FloatEasingCoroutine : EasingCoroutine<float>
    {
        public FloatEasingCoroutine(float from, float to, float duration, float delay, UnityAction<float> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Lerp(float a, float b, float t)
        {
            return Mathf.LerpUnclamped(a, b, t);
        }
    }

    public class VectorEasingCoroutine3 : EasingCoroutine<Vector3>
    {
        public VectorEasingCoroutine3(Vector3 from, Vector3 to, float duration, float delay, UnityAction<Vector3> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Vector3.LerpUnclamped(a, b, t);
        }
    }

    public class VectorEasingCoroutine2 : EasingCoroutine<Vector2>
    {
        public VectorEasingCoroutine2(Vector2 from, Vector2 to, float duration, float delay, UnityAction<Vector2> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return Vector2.LerpUnclamped(a, b, t);
        }
    }

    public class ColorEasingCoroutine : EasingCoroutine<Color>
    {
        public ColorEasingCoroutine(Color from, Color to, float duration, float delay, UnityAction<Color> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Color Lerp(Color a, Color b, float t)
        {
            return Color.LerpUnclamped(a, b, t);
        }
    }
}
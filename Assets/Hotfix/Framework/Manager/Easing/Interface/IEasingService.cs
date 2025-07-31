using System;
using UnityEngine.Events;

namespace LccHotfix
{
    public interface IEasingService : IService
    {
        IEasingCoroutine DoFloat(float from, float to, float duration, UnityAction<float> action, float delay = 0);

        IEasingCoroutine DoAfter(float seconds, UnityAction action, bool unscaledTime = false);

        IEasingCoroutine DoAfter(Func<bool> condition);

        IEasingCoroutine DoNextFrame();

        IEasingCoroutine DoNextFrame(UnityAction action);

        IEasingCoroutine DoNextFixedFrame();
    }
}
using System;
using System.Collections;

namespace LccHotfix
{
    public interface ICoroutineService : IService
    {
        ICoroutineHelper CoroutineHelper { get; set; }
        void SetCoroutineHelper(ICoroutineHelper coroutineHelper);
        CoroutineHandler StartCoroutine(ICoroutine owner, IEnumerator coroutine);
        void StopCoroutine(CoroutineHandler handler);

        void PauseCoroutine(CoroutineHandler handler);

        void ResumeCoroutine(CoroutineHandler handler);

        void StopAllCoroutines(ICoroutine owner);

        void StopAllTypeCoroutines();
    }
}
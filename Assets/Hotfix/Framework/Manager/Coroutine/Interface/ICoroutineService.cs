using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using UnityEngine;

namespace LccHotfix
{
    public interface ICoroutineService : IService
    {
        CoroutineHandler StartCoroutine(ICoroutine owner, IEnumerator coroutine);

        void StopCoroutine(CoroutineHandler handler);

        void PauseCoroutine(CoroutineHandler handler);

        void ResumeCoroutine(CoroutineHandler handler);

        void StopAllCoroutines(ICoroutine owner);

        void StopAllTypeCoroutines();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultCoroutineHelper : ICoroutineHelper
{
    public void StartCoroutine(IEnumerator coroutine)
    {
        Object.FindObjectOfType<LccModel.Launcher>().StartCoroutine(coroutine);
    }
}

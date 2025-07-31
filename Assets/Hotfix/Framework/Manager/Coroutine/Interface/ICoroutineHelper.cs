using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoroutineHelper
{
    void StartCoroutine(IEnumerator coroutine);
}
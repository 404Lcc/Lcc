using System.Collections;
using System.Collections.Generic;
using LccHotfix;
using UnityEngine;

public interface ISceneHelper
{
    IEnumerator ShowSceneLoading(LoadingType loadType);
    void ResetSpeed();
    void UpdateLoadingTime(LoadSceneHandler handler);
    void UnloadAllWindow(LoadSceneHandler last, LoadSceneHandler cur);
    void OpenChangeScenePanel(LoadSceneHandler handler);
}
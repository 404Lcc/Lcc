using UnityEngine;
using System.Collections;
using LccModel;
using YooAsset;

public class Launcher : SingletonMono<Launcher>
{
    private LauncherOperation _launcherOperation;
    
    void Start()
    {
        Debug.LogWarning("Launcher Start");
        
        StartCoroutine(RunLaunch());
    }

    private IEnumerator RunLaunch()
    {
        YooAssets.Initialize();
        
        _launcherOperation = new LauncherOperation();
        YooAssets.StartOperation(_launcherOperation);
        yield return _launcherOperation;
        
        if (_launcherOperation.Status == EOperationStatus.Succeed)
        {
            Debug.LogWarning("[Launch]launcher succeed");
        }
        else
        {
            Debug.LogError("[Launch]launcher error : " + _launcherOperation.Error);
        }
        _launcherOperation = null;
    }
}

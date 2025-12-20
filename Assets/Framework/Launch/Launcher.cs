using UnityEngine;
using System.Collections;
using System.Reflection;
using LccModel;
using YooAsset;

public partial class Launcher : SingletonMono<Launcher>
{
    private LauncherOperation _launcherOperation;
    public Assembly HotfixAssembly { get; set; }

    void Start()
    {
        StartLaunch();
    }

    public void StartLaunch()
    {
        Debug.LogWarning("[Launch] Start");
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
            Debug.LogWarning("[Launch] launcher succeed");
        }
        else
        {
            Debug.LogError("[Launch] launcher error : " + _launcherOperation.Error);
        }

        _launcherOperation = null;
    }
}
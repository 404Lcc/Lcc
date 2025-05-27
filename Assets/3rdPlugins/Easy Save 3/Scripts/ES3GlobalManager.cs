using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
public class ES3GlobalManager : MonoBehaviour
{
    // Indicates whether an event has indicated that the cache should be stored to file at the end of this frame.
    bool storeCache = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Run()
    {
        var gameObject = new GameObject("Easy Save 3 Global Manager");
        gameObject.AddComponent<ES3GlobalManager>();
        DontDestroyOnLoad(gameObject);

        if(ES3Settings.defaultSettings.autoCacheDefaultFile)
            ES3.CacheFile();
    }

    public IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if(ES3Settings.defaultSettings.location == ES3.Location.Cache && ES3Settings.defaultSettings.storeCacheAtEndOfEveryFrame || storeCache)
            {
                ES3File.StoreAll();
                storeCache = false;
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (ES3Settings.defaultSettings.storeCacheOnApplicationQuit)
            storeCache = true;
    }

    private void OnApplicationPause(bool paused)
    {
        if ((ES3Settings.defaultSettings.storeCacheOnApplicationPause || (Application.isMobilePlatform && ES3Settings.defaultSettings.storeCacheOnApplicationQuit)) && paused)
            storeCache = true;
    }
}

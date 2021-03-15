using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LccModel
{
    public class SceneLoadManager : Singleton<SceneLoadManager>
    {
        public AsyncOperation async;
        public event Action Callback;
        public int process;
        private string GetAssetPath(string name, params string[] types)
        {
            if (types.Length == 0) return name;
            string path = string.Empty;
            for (int i = 0; i < types.Length; i++)
            {
                path = $"{path}{types[i]}/";
                if (i == types.Length - 1)
                {
                    path = $"{path}{name}";
                }
            }
            return path;
        }
        private IEnumerator LoadScene(string name, string suffix, bool isAssetBundle, params string[] types)
        {
            yield return new WaitForSecondsRealtime(0.1f);
#if AssetBundle
            if (isAssetBundle)
            {
#if !UNITY_EDITOR
                string path = GetAssetPath(name, types);
                AssetBundleManager.Instance.LoadAsset($"Assets/Bundles/{path}{suffix}");
                async = SceneManager.LoadSceneAsync($"Assets/Bundles/{path}{suffix}");
                async.allowSceneActivation = false;
                //AsyncOperationHandle<SceneInstance> handler = Addressables.LoadSceneAsync($"Assets/Bundles/{path}{suffix}");
#else
                async = SceneManager.LoadSceneAsync(name);
                async.allowSceneActivation = false;
#endif
            }
            else
            {
                async = SceneManager.LoadSceneAsync(name);
                async.allowSceneActivation = false;
            }
#else
            async = SceneManager.LoadSceneAsync(name);
            async.allowSceneActivation = false;
#endif
            yield return new WaitForSecondsRealtime(0.1f);
            while (process < (int)(async.progress * 100))
            {
                process++;
                yield return null;
            }
            while (process < 100)
            {
                process++;
                yield return null;
            }
            async.allowSceneActivation = true;
            yield return new WaitForSecondsRealtime(0.1f);
            Callback?.Invoke();
        }
        public void LoadScene(string name, string suffix, bool isAssetBundle, Action callback, params string[] types)
        {
            Callback = callback;
            process = 0;
            SceneManager.LoadScene(SceneName.Load);
            StartCoroutine(LoadScene(name, suffix, isAssetBundle, types));
        }
        public void LoadScene(string name, bool isAssetBundle, Action callback, params string[] types)
        {
            Callback = callback;
            process = 0;
            SceneManager.LoadScene(SceneName.Load);
            StartCoroutine(LoadScene(name, ".unity", isAssetBundle, types));
        }
    }
}
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
        public override void Update()
        {
            if (async == null) return;
            if (async.progress < 0.9f)
            {
                process = (int)(async.progress * 100);
            }
            else
            {
                process = 100;
            }
            if (async.isDone)
            {
                Callback?.Invoke();
                async.allowSceneActivation = true;
                async = null;
                Callback = null;
                process = 0;
            }
        }
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
#if AssetBundle
            if (isAssetBundle)
            {
#if !UNITY_EDITOR
                string path = GetAssetPath(name, types);
                AssetBundleManager.Instance.LoadAsset($"Assets/Bundles/{path}{suffix}");
                async = SceneManager.LoadSceneAsync($"Assets/Bundles/{path}{suffix}");
                async.allowSceneActivation = false;
                //AsyncOperationHandle<SceneInstance> handler = Addressables.LoadSceneAsync($"Assets/Bundles/{path}{suffix}");
                //yield return handler;
#else
                async = SceneManager.LoadSceneAsync(name);
                async.allowSceneActivation = false;
                yield return async;
#endif
            }
            else
            {
                async = SceneManager.LoadSceneAsync(name);
                async.allowSceneActivation = false;
                yield return async;
            }
#else
            async = SceneManager.LoadSceneAsync(name);
            async.allowSceneActivation = false;
            yield return async;
#endif
        }
        public void LoadScene(string name, string suffix, bool isAssetBundle, Action callback, params string[] types)
        {
            Callback = callback;
            SceneManager.LoadScene(SceneName.Load);
            StartCoroutine(LoadScene(name, suffix, isAssetBundle, types));
        }
        public void LoadScene(string name, bool isAssetBundle, Action callback, params string[] types)
        {
            Callback = callback;
            SceneManager.LoadScene(SceneName.Load);
            StartCoroutine(LoadScene(name, ".unity", isAssetBundle, types));
        }
    }
}
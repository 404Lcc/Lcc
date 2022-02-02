using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LccModel
{
    public class SceneLoadManager : Singleton<SceneLoadManager>
    {
        public AsyncOperation async;
        public bool isLoading;
        private int _process;
        private int _toProcess;
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
        private async ETTask LoadScene(string name, string suffix, bool isAssetBundle, params string[] types)
        {
            _toProcess = 0;
#if AssetBundle
            if (isAssetBundle)
            {
#if !UNITY_EDITOR
                string path = GetAssetPath(name, types);
                AssetBundleManager.Instance.LoadAsset($"Assets/Bundles/{path}{suffix}");
                async = SceneManager.LoadSceneAsync($"Assets/Bundles/{path}{suffix}");
                async.allowSceneActivation = false;
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
            while (async.progress < 0.9f)
            {
                _toProcess = (int)(async.progress * 100);
                while (_process < _toProcess)
                {
                    ++_process;
                }
                await Task.Delay(10);
            }
            _toProcess = 100;
            while (_process < _toProcess)
            {
                ++_process;
                await Task.Delay(10);
            }
            async.allowSceneActivation = true;
        }
        public async ETTask LoadScene(string name, bool isAssetBundle, params string[] types)
        {
            isLoading = true;
            _process = 0;
            SceneManager.LoadScene(SceneName.Load);
            await LoadScene(name, ".unity", isAssetBundle, types);
            isLoading = false;
            _process = 0;
        }
        public int GetLoadProcess()
        {
            if (isLoading)
            {
                return _process;
            }
            else
            {
                return 0;
            }
        }
    }
}
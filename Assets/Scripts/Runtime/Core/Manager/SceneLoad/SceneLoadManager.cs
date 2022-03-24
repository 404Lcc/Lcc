using BM;
using ET;
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
        private string GetAssetPath(string name, string suffix, params string[] types)
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
            return $"Assets/Bundles/{path}{suffix}";
        }
        private async ETTask LoadSceneAsync(string name, string suffix, params string[] types)
        {
            _toProcess = 0;
            string path = GetAssetPath(name, suffix, types);
            await AssetComponent.LoadSceneAsync(path);
            async = SceneManager.LoadSceneAsync(path);
            async.allowSceneActivation = false;
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
        public async ETTask LoadSceneAsync(string name, params string[] types)
        {
            isLoading = true;
            _process = 0;
            SceneManager.LoadScene(SceneName.Load);
            await LoadSceneAsync(name, AssetSuffix.Unity, types);
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
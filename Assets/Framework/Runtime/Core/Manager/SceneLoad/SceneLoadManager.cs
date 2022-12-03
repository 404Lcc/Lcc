using BM;
using ET;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LccModel
{
    public class SceneLoadManager : AObjectBase
    {
        public static SceneLoadManager Instance { get; set; }
        private AsyncOperation _async;
        public bool isLoading;
        private int _process;
        private int _toProcess;
        public override void Awake()
        {
            base.Awake();


            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            _async = null;
            isLoading = false;
            _process = 0;
            _toProcess = 0;

            Instance = null;
        }
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
            string path = GetAssetPath(name, suffix, types);
            await AssetComponent.LoadSceneAsync(path);
            _async = SceneManager.LoadSceneAsync(path);
            _async.allowSceneActivation = false;
            while (_async.progress < 0.9f)
            {
                _toProcess = (int)(_async.progress * 100);
                while (_process < _toProcess)
                {
                    _process++;
                }
                await TimerManager.Instance.WaitFrameAsync(1);
            }
            _toProcess = 100;
            while (_process < _toProcess)
            {
                _process++;
                await TimerManager.Instance.WaitFrameAsync(1);
            }
            _async.allowSceneActivation = true;
        }
        public async ETTask LoadSceneAsync(string name, params string[] types)
        {
            isLoading = true;
            _process = 0;
            _toProcess = 0;
            SceneManager.LoadScene(SceneName.Load);
            await LoadSceneAsync(name, AssetSuffix.Unity, types);
            isLoading = false;
            _process = 0;
            _toProcess = 0;
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
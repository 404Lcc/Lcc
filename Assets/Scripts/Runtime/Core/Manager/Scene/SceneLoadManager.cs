using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LccModel
{
    public class SceneLoadManager : Singleton<SceneLoadManager>
    {
        public AsyncOperation async;
        public Action complete;
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
                complete?.Invoke();
                async.allowSceneActivation = true;
                async = null;
                complete = null;
                process = 0;
            }
        }
        public void LoadScene(string name, Action complete, params string[] types)
        {
            this.complete = complete;
            SceneManager.LoadScene(SceneName.Load);
#if AssetBundle
            if (types.Length == 0) return;
            string path = string.Empty;
            for (int i = 0; i < types.Length; i++)
            {
                path += types[i] + "/";
                if (i == types.Length - 1)
                {
                    path += name;
                }
            }
#else
            StartCoroutine(LoadScene(name));
#endif
        }
        public IEnumerator LoadScene(string name)
        {
            async = SceneManager.LoadSceneAsync(name);
            async.allowSceneActivation = false;
            yield return async;
        }
    }
}
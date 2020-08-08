//using libx;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hotfix
{
    public class LoadSceneManager : MonoBehaviour
    {
        private AsyncOperation async;
        private uint process;
        public int Process
        {
            get
            {
                return (int)process;
            }
        }
        void Start()
        {
        }
        void Update()
        {
            if (async == null) return;
            if (async.progress < 0.9f)
            {
                process = (uint)async.progress * 100;
            }
            else
            {
                process = 100;
            }
            if (async.isDone)
            {
                if (LoadData.bloadpanel) IO.panelManager.ClearPanel(PanelType.Load);
                LoadData.action?.Invoke();
                IO.panelManager.OpenPanel(LoadData.open.ToArray());
                IO.panelManager.ClearPanel(LoadData.clear.ToArray());
                LoadData.loadid = 0;
                LoadData.open.Clear();
                LoadData.clear.Clear();
                LoadData.bloadpanel = false;
                LoadData.action = null;
                async.allowSceneActivation = true;
                async = null;
                process = 0;
            }
        }
        public void StartLoadScene()
        {
            StartCoroutine(LoadScene());
        }
        public void LoadSceneData(string name, string suffix, params AssetType[] types)
        {
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
            //StartCoroutine(LoadSceneAsync("Assets/Resources/" + path + suffix, false, (SceneAssetRequest request) =>
            //{
            //    while (!request.isDone)
            //    {
            //        process = (uint)request.progress;
            //    }
            //    if (request.isDone)
            //    {
            //        if (LoadData.bloadpanel) IO.panelManager.ClearPanel(PanelType.Load);
            //        LoadData.action?.Invoke();
            //        IO.panelManager.OpenPanel(LoadData.open.ToArray());
            //        IO.panelManager.ClearPanel(LoadData.clear.ToArray());
            //        LoadData.loadid = 0;
            //        LoadData.open.Clear();
            //        LoadData.clear.Clear();
            //        LoadData.bloadpanel = false;
            //        LoadData.action = null;
            //    }
            //}));
        }
        public IEnumerator LoadScene()
        {
            async = SceneManager.LoadSceneAsync(LoadData.loadid);
            async.allowSceneActivation = false;
            yield return async;
        }
        //public IEnumerator LoadSceneAsync(string path, bool additive, Action<SceneAssetRequest> action)
        //{
        //    SceneAssetRequest request = Assets.LoadSceneAsync(path, additive);
        //    yield return request;
        //    action?.Invoke(request);
        //}
    }
}
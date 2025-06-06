using LccModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class SceneManager : Module, ICoroutine
    {
        public static SceneManager Instance => Entry.GetModule<SceneManager>();


        private Dictionary<SceneType, LoadSceneHandler> _loadSceneHandlerDict = new Dictionary<SceneType, LoadSceneHandler>();
        public SceneManager()
        {
            foreach (Type item in CodeTypesManager.Instance.GetTypes(typeof(SceneStateAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(SceneStateAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    SceneStateAttribute sceneStateAttribute = (SceneStateAttribute)atts[0];

                    LoadSceneHandler sceneState = (LoadSceneHandler)Activator.CreateInstance(item);
                    sceneState.sceneType = sceneStateAttribute.sceneType;

                    _loadSceneHandlerDict.Add(sceneStateAttribute.sceneType, sceneState);
                }
            }
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            UpdateLoadingTime();
            if (curSceneHandler != null && !curSceneHandler.IsLoading)
            {
                curSceneHandler.Tick();
            }
        }

        internal override void LateUpdate()
        {
            base.LateUpdate();
            if (curSceneHandler != null && !curSceneHandler.IsLoading)
            {
                curSceneHandler.LateUpdate();
            }
        }

        internal override void Shutdown()
        {
            this.StopAllCoroutines();
            _loadSceneHandlerDict.Clear();
            _inLoading = false;
            preSceneHandler = null;
            curSceneHandler = null;
        }

        #region Load Scene
        private bool _inLoading;

        private LoadSceneHandler preSceneHandler;
        private LoadSceneHandler curSceneHandler;

        public SceneType curState
        {
            get
            {
                if (curSceneHandler != null)
                    return curSceneHandler.sceneType;
                return 0;
            }
        }
        public SceneType preState
        {
            get
            {
                if (preSceneHandler != null)
                    return preSceneHandler.sceneType;
                return 0;
            }
        }
        public bool IsLoading
        {
            get
            {
                if (!Init.HotfixGameStarted) return true;
                if (_inLoading) return true;
                if (curSceneHandler == null) return true;
                return curSceneHandler.IsLoading;
            }
        }

        public LoadSceneHandler GetScene(SceneType type)
        {
            if (type == SceneType.None)
            {
                return null;
            }
            LoadSceneHandler handler = _loadSceneHandlerDict[type];
            return handler;
        }

        public void ChangeScene(LoadSceneHandler handler)
        {
            if (handler == null) return;

            if (curSceneHandler != null && curSceneHandler.sceneType == handler.sceneType && !curSceneHandler.IsLoading)
                return;
            if (!handler.SceneEnterStateHandler())
                return;

            if (curSceneHandler != null)
                preSceneHandler = curSceneHandler;
            curSceneHandler = handler;
            Log.Info($"ChangeScene： scene type === {curSceneHandler.sceneType.ToString()} loading type ==== {((LoadingType)curSceneHandler.loadType).ToString()}");


            LccModel.Launcher.Instance.SetGameSpeed(1);
            LccModel.Launcher.Instance.ChangeFPS();

            _inLoading = false;
            handler.IsLoading = true;
            handler.IsCleanup = false;
            handler.startLoadTime = Time.realtimeSinceStartup;

            handler.SceneLoadHandler();
        }

        public void ChangeScene(SceneType type)
        {
            if (type == SceneType.None)
            {
                return;
            }
            LoadSceneHandler handler = _loadSceneHandlerDict[type];
            if (handler == null)
            {
                return;
            }
            ChangeScene(handler);
        }

        public IEnumerator ShowSceneLoading(LoadingType loadType)
        {
            UILoadingPanel loadingPanel = null;
            switch (loadType)
            {
                case LoadingType.Normal:
                    loadingPanel = UILoadingPanel.Instance;
                    loadingPanel.Show(string.Empty);
                    loadingPanel.UpdateLoadingPercent(0, 5);
                    yield return null;
                    break;
                case LoadingType.Fast:
                    UIForeGroundPanel.Instance.FadeOut(1.5f);
                    yield return null;
                    break;
            }
            BeginLoad();
        }


        public void BeginLoad()
        {
            Log.Info($"BeginLoad： scene type === {curSceneHandler.sceneType.ToString()} loading type ==== {((LoadingType)curSceneHandler.loadType).ToString()}");
            Instance.StartCoroutine(Instance.UnloadSceneCoroutine());
        }

        private IEnumerator UnloadSceneCoroutine()
        {
            if (curSceneHandler == null) yield break;
            //移除旧的
            if (preSceneHandler != null)
            {
                preSceneHandler.SceneExitHandler();
                preSceneHandler.IsCleanup = true;
                yield return null;
            }
            Entry.GetModule<WindowManager>().ShowMaskBox((int)MaskType.WINDOW_ANIM, false);
            Entry.GetModule<WindowManager>().CloseAllWindow();
            yield return null;
            if (curSceneHandler.deepClean || (preSceneHandler != null && preSceneHandler.deepClean))
                Entry.GetModule<WindowManager>().ReleaseAllWindow(ReleaseType.DEEPLY);
            else
                Entry.GetModule<WindowManager>().ReleaseAllWindow(ReleaseType.CHANGE_SCENE);
            yield return null;
            System.GC.Collect();
            yield return null;
            Log.Info($"UnloadSceneCoroutine： scene type === {curSceneHandler.sceneType.ToString()} loading type ==== {((LoadingType)curSceneHandler.loadType).ToString()}");
            curSceneHandler.SceneStartHandler();
        }

        public void CleanScene()
        {
            //如果上一个还没清理完成 直接清理一下
            if (preSceneHandler != null)
            {
                if (!preSceneHandler.IsCleanup)
                {
                    preSceneHandler.SceneExitHandler();
                }
            }

            if (curSceneHandler != null)
            {
                curSceneHandler.SceneExitHandler();
                curSceneHandler = null;
            }
            preSceneHandler = null;
        }

        private void UpdateLoadingTime()
        {
            if (curSceneHandler != null && curSceneHandler.IsLoading)
            {
                if (Time.realtimeSinceStartup - curSceneHandler.startLoadTime > 150)
                {
                    Init.ReturnToStart();
                }
            }
        }
        #endregion

        #region 切场景界面
        public void OpenChangeScenePanel()
        {
            if (curSceneHandler == null || curSceneHandler.turnNode == null) return;
            WNode.TurnNode node = curSceneHandler.turnNode;
            if (string.IsNullOrEmpty(node.nodeName))
                return;
            if (Entry.GetModule<WindowManager>().OpenSpecialWindow(node))
                return;
            if (node.nodeType == NodeType.ROOT)
            {
                Entry.GetModule<WindowManager>().OpenRoot(node.nodeName, node.nodeParam);
            }
            else
            {
                Entry.GetModule<WindowManager>().OpenWindow(node.nodeName, node.nodeParam);
            }

            curSceneHandler.turnNode = null;
        }

        public void CleanChangeSceneParam()
        {
            if (curSceneHandler != null)
                curSceneHandler.turnNode = null;
        }
        #endregion

    }
}
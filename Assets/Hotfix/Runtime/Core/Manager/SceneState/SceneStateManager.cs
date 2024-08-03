using cfg;
using LccModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SceneStateManager : AObjectBase
    {
        public static SceneStateManager Instance { get; set; }

        private bool _forceStop;
        private SceneState preSceneHandler;
        private SceneState curSceneHandler;


        private Dictionary<SceneStateType, SceneState> _sceneStateDict = new Dictionary<SceneStateType, SceneState>();


        public override void Awake()
        {
            base.Awake();

            Instance = this;

            _forceStop = false;
            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(SceneStateAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(SceneStateAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    SceneStateAttribute sceneStateAttribute = (SceneStateAttribute)atts[0];

                    SceneState sceneState = (SceneState)Activator.CreateInstance(item);
                    sceneState.sceneType = sceneStateAttribute.sceneType;

                    _sceneStateDict.Add(sceneStateAttribute.sceneType, sceneState);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;

            _forceStop = true;
            _sceneStateDict.Clear();
        }

        public void OpenChangeScenePanel()
        {
            var curScene = GetCurrentState();
            if (curScene == null || curScene.jumpNode == null)
            {
                return;
            }
            JumpNode node = curScene.jumpNode;
            if (node.nodeType == PanelType.None)
            {
                return;
            }
            if (JumpManager.Instance.OpenSpecialWindow(node))
            {
                return;
            }

            if (node.depend != null)
            {
                var dependData = new ShowPanelData(false, true, node.depend.nodeParam, true, false, true);
                PanelManager.Instance.ShowPanel(node.depend.nodeType, dependData);
            }
            var data = new ShowPanelData(false, true, node.nodeParam, true, false, true);
            PanelManager.Instance.ShowPanel(node.nodeType, data);
            curScene.jumpNode = null;
        }

        public void CleanChangeSceneParam()
        {
            var curScene = GetCurrentState();
            if (curScene != null)
            {
                curScene.jumpNode = null;
            }
        }


        public void ChangeScene(SceneStateType type, object[] args = null)
        {
            if (type != SceneStateType.None)
            {
                SceneState handler = _sceneStateDict[type];
                if (handler == null)
                {
                    return;
                }
                if (curSceneHandler != null && curSceneHandler.sceneType == handler.sceneType && !curSceneHandler.IsLoading)
                {
                    return;
                }

                if (curSceneHandler != null)
                {
                    preSceneHandler = curSceneHandler;
                }
                curSceneHandler = handler;
                LogHelper.Info($"ChangeScene： scene type === {curSceneHandler.sceneType} loading type ==== {curSceneHandler.loadType}");


                LccModel.Init.SetGameSpeed(1);
                LccModel.Init.ChangeFPS();

                handler.IsLoading = true;
                handler.startLoadTime = UnityEngine.Time.realtimeSinceStartup;
                handler.IsLoading = true;
                if (!handler.SceneLoadHandler())
                {
                    StartCoroutine(ShowSceneLoading(handler.loadType, args));
                }
                else
                {
                    BeginLoad(args);
                }

            }


        }

        private IEnumerator ShowSceneLoading(LoadingType loadType, object[] args = null)
        {
            switch (loadType)
            {
                case LoadingType.Normal:

                    yield return null;
                    break;
                case LoadingType.Fast:

                    yield return null;
                    break;
            }
            BeginLoad(args);
        }

        public SceneState GetState(SceneStateType type)
        {
            if (!_sceneStateDict.ContainsKey(type)) return null;
            return _sceneStateDict[type];
        }

        public SceneState GetCurrentState()
        {
            return curSceneHandler;
        }

        public void Update()
        {
            if (curSceneHandler == null) return;
            if (_forceStop) return;

            curSceneHandler.Tick();
        }


        #region Load Scene


        public SceneStateType CurState
        {
            get
            {
                if (curSceneHandler != null) return curSceneHandler.sceneType;
                return 0;
            }
        }
        public SceneStateType PreState
        {
            get
            {
                if (preSceneHandler != null) return preSceneHandler.sceneType;
                return 0;
            }
        }
        public bool IsLoading
        {
            get
            {
                if (!LccModel.Init.gameStarted) return true;
                if (curSceneHandler == null) return true;
                return curSceneHandler.IsLoading;
            }
        }




        public void BeginLoad(object[] args = null)
        {
            LogHelper.Info($"BeginLoad： scene type === {curSceneHandler.sceneType} loading type ==== {curSceneHandler.loadType}");
            Instance.StartCoroutine(Instance.UnloadSceneCoroutine(args));
        }

        private IEnumerator UnloadSceneCoroutine(object[] args = null)
        {
            if (curSceneHandler == null)
            {
                yield break;
            }


            PanelManager.Instance.HideAllShownPanel();


            //移除旧的
            if (preSceneHandler != null)
            {
                preSceneHandler.OnExit();
                yield return null;
            }


            //关闭遮罩 todo


            yield return null;

            GC.Collect();
            yield return null;
            LogHelper.Info($"UnloadSceneCoroutine： scene type === {curSceneHandler.sceneType} loading type ==== {curSceneHandler.loadType}");
            curSceneHandler.OnEnter(args);
        }

        public void CleanScene()
        {
            if (curSceneHandler != null)
            {
                curSceneHandler.OnExit();
                curSceneHandler = null;
            }
            preSceneHandler = null;
        }

        private void UpdateLoadingTime()
        {
            if (curSceneHandler != null && curSceneHandler.IsLoading)
            {
                if (UnityEngine.Time.realtimeSinceStartup - curSceneHandler.startLoadTime > 150)
                {

                }
            }
        }


        #endregion

    }
}
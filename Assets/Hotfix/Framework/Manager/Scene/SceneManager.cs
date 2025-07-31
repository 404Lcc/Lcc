using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class SceneManager : Module, ISceneService, ICoroutine
    {
        private Dictionary<SceneType, LoadSceneHandler> _loadSceneHandlerDict = new Dictionary<SceneType, LoadSceneHandler>();
        private bool _inLoading;
        private LoadSceneHandler _curSceneHandler;
        private ISceneHelper _sceneHelper;

        public SceneType CurState
        {
            get
            {
                if (_curSceneHandler != null)
                    return _curSceneHandler.sceneType;
                return SceneType.None;
            }
        }

        public bool IsLoading
        {
            get
            {
                if (!Init.HotfixGameStarted)
                    return true;
                if (_inLoading)
                    return true;
                if (_curSceneHandler == null)
                    return true;
                return _curSceneHandler.IsLoading;
            }
        }

        public SceneManager()
        {
            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(SceneStateAttribute)))
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
            if (_curSceneHandler == null)
                return;

            if (!_curSceneHandler.IsLoading)
            {
                _curSceneHandler.Tick();
            }

            _sceneHelper.UpdateLoadingTime(_curSceneHandler);
        }

        internal override void LateUpdate()
        {
            base.LateUpdate();
            
            if (_curSceneHandler == null)
                return;
            
            if (!_curSceneHandler.IsLoading)
            {
                _curSceneHandler.LateUpdate();
            }
        }

        internal override void Shutdown()
        {
            this.StopAllCoroutines();
            _loadSceneHandlerDict.Clear();
            _inLoading = false;
            _curSceneHandler = null;
        }

        public void SetSceneHelper(ISceneHelper sceneHelper)
        {
            this._sceneHelper = sceneHelper;
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
        
        private void ChangeScene(LoadSceneHandler handler)
        {
            if (handler == null)
                return;

            if (_curSceneHandler != null && _curSceneHandler.sceneType == handler.sceneType)
                return;

            if (!handler.SceneEnterStateHandler())
                return;

            LoadSceneHandler last = null;
            if (_curSceneHandler != null)
            {
                last = _curSceneHandler;
            }

            _curSceneHandler = handler;

            Log.Info($"ChangeScene： scene type === {_curSceneHandler.sceneType.ToString()} loading type ==== {_curSceneHandler.loadType.ToString()}");

            _sceneHelper.ResetSpeed();

            _inLoading = false;
            handler.IsLoading = true;
            handler.IsCleanup = false;
            handler.startLoadTime = Time.realtimeSinceStartup;
            handler.SceneLoadHandler();
            
            Log.Info($"BeginLoad： scene type === {_curSceneHandler.sceneType.ToString()} loading type ==== {_curSceneHandler.loadType.ToString()}");
            this.StartCoroutine(UnloadSceneCoroutine(last));
        }

        private IEnumerator UnloadSceneCoroutine(LoadSceneHandler last)
        {
            if (_curSceneHandler == null)
                yield break;

            //移除旧的
            if (last != null)
            {
                last.SceneExitHandler();
                last.IsCleanup = true;
                yield return null;
            }

            _sceneHelper.UnloadAllWindow(last, _curSceneHandler);
            
            yield return null;

            GC.Collect();

            yield return null;

            Log.Info($"UnloadSceneCoroutine： scene type === {_curSceneHandler.sceneType.ToString()} loading type ==== {((LoadingType)_curSceneHandler.loadType).ToString()}");
            _curSceneHandler.SceneStartHandler();
        }

        public void CleanScene()
        {
            if (_curSceneHandler != null)
            {
                _curSceneHandler.SceneExitHandler();
                _curSceneHandler = null;
            }
        }

        #region 切场景界面

        public void OpenChangeScenePanel()
        {
            _sceneHelper.OpenChangeScenePanel(_curSceneHandler);
        }

        public void CleanChangeSceneParam()
        {
            if (_curSceneHandler != null)
                _curSceneHandler.turnNode = null;
        }

        #endregion
    }
}
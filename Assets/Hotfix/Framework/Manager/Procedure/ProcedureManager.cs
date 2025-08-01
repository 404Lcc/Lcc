using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class ProcedureManager : Module, IProcedureService, ICoroutine
    {
        private Dictionary<ProcedureType, LoadProcedureHandler> _loadSceneHandlerDict = new Dictionary<ProcedureType, LoadProcedureHandler>();
        private bool _inLoading;
        private LoadProcedureHandler _curSceneHandler;
        private IProcedureHelper _sceneHelper;

        public ProcedureType CurState
        {
            get
            {
                if (_curSceneHandler != null)
                    return _curSceneHandler.procedureType;
                return ProcedureType.None;
            }
        }

        public bool IsLoading
        {
            get
            {
                if (_inLoading)
                    return true;
                if (_curSceneHandler == null)
                    return true;
                return _curSceneHandler.IsLoading;
            }
        }

        public ProcedureManager()
        {
            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(ProcedureAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ProcedureAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    ProcedureAttribute sceneStateAttribute = (ProcedureAttribute)atts[0];

                    LoadProcedureHandler sceneState = (LoadProcedureHandler)Activator.CreateInstance(item);
                    sceneState.procedureType = sceneStateAttribute.type;

                    _loadSceneHandlerDict.Add(sceneStateAttribute.type, sceneState);
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

        public void SetProcedureHelper(IProcedureHelper sceneHelper)
        {
            this._sceneHelper = sceneHelper;
        }
        
        public LoadProcedureHandler GetProcedure(ProcedureType type)
        {
            if (type == ProcedureType.None)
            {
                return null;
            }

            LoadProcedureHandler handler = _loadSceneHandlerDict[type];
            return handler;
        }
        
        public void ChangeProcedure(ProcedureType type)
        {
            if (type == ProcedureType.None)
            {
                return;
            }

            LoadProcedureHandler handler = _loadSceneHandlerDict[type];
            if (handler == null)
            {
                return;
            }

            ChangeScene(handler);
        }
        
        private void ChangeScene(LoadProcedureHandler handler)
        {
            if (handler == null)
                return;

            if (_curSceneHandler != null && _curSceneHandler.procedureType == handler.procedureType)
                return;

            if (!handler.ProcedureEnterStateHandler())
                return;

            LoadProcedureHandler last = null;
            if (_curSceneHandler != null)
            {
                last = _curSceneHandler;
            }

            _curSceneHandler = handler;

            Log.Info($"ChangeScene： scene type === {_curSceneHandler.procedureType.ToString()} loading type ==== {_curSceneHandler.loadType.ToString()}");

            _sceneHelper.ResetSpeed();

            _inLoading = false;
            handler.IsLoading = true;
            handler.IsCleanup = false;
            handler.startLoadTime = Time.realtimeSinceStartup;
            handler.ProcedureLoadHandler();
            
            Log.Info($"BeginLoad： scene type === {_curSceneHandler.procedureType.ToString()} loading type ==== {_curSceneHandler.loadType.ToString()}");
            this.StartCoroutine(UnloadSceneCoroutine(last));
        }

        private IEnumerator UnloadSceneCoroutine(LoadProcedureHandler last)
        {
            if (_curSceneHandler == null)
                yield break;

            //移除旧的
            if (last != null)
            {
                last.ProcedureExitHandler();
                last.IsCleanup = true;
                yield return null;
            }

            _sceneHelper.UnloadAllPanel(last, _curSceneHandler);
            
            yield return null;

            GC.Collect();

            yield return null;

            Log.Info($"UnloadSceneCoroutine： scene type === {_curSceneHandler.procedureType.ToString()} loading type ==== {((LoadingType)_curSceneHandler.loadType).ToString()}");
            _curSceneHandler.ProcedureStartHandler();
        }

        public void CleanProcedure()
        {
            if (_curSceneHandler != null)
            {
                _curSceneHandler.ProcedureExitHandler();
                _curSceneHandler = null;
            }
        }

        #region 切流程界面

        public void OpenChangeProcedurePanel()
        {
            _sceneHelper.OpenChangeProcedurePanel(_curSceneHandler);
        }

        public void CleanChangeProcedureParam()
        {
            if (_curSceneHandler != null)
                _curSceneHandler.turnNode = null;
        }

        #endregion
    }
}
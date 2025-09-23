using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class ProcedureManager : Module, IProcedureService, ICoroutine
    {
        private Dictionary<int, LoadProcedureHandler> _loadProcedureHandlerDict = new Dictionary<int, LoadProcedureHandler>();
        private bool _inLoading;
        private LoadProcedureHandler _curProcedureHandler;
        private IProcedureHelper _procedureHelper;

        public int CurState
        {
            get
            {
                if (_curProcedureHandler != null)
                    return _curProcedureHandler.procedureType;
                return 0;
            }
        }

        public bool IsLoading
        {
            get
            {
                if (_inLoading)
                    return true;
                if (_curProcedureHandler == null)
                    return true;
                return _curProcedureHandler.IsLoading;
            }
        }

        public ProcedureManager()
        {
            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(ProcedureAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ProcedureAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    ProcedureAttribute procedureAttribute = (ProcedureAttribute)atts[0];
                    LoadProcedureHandler handler = (LoadProcedureHandler)Activator.CreateInstance(item);
                    if (handler.procedureType == 0)
                    {
                        Debug.LogError("流程类型不能为0 " + item.Name);
                        continue;
                    }

                    _loadProcedureHandlerDict.Add(handler.procedureType, handler);
                }
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_curProcedureHandler == null)
                return;

            if (!_curProcedureHandler.IsLoading)
            {
                _curProcedureHandler.Tick();
            }

            _procedureHelper.UpdateLoadingTime(_curProcedureHandler);
        }

        internal override void LateUpdate()
        {
            base.LateUpdate();

            if (_curProcedureHandler == null)
                return;

            if (!_curProcedureHandler.IsLoading)
            {
                _curProcedureHandler.LateUpdate();
            }
        }

        internal override void Shutdown()
        {
            this.StopAllCoroutines();
            _loadProcedureHandlerDict.Clear();
            _inLoading = false;
            _curProcedureHandler = null;
        }

        public void SetProcedureHelper(IProcedureHelper procedureHelper)
        {
            this._procedureHelper = procedureHelper;
        }

        public LoadProcedureHandler GetProcedure(int type)
        {
            if (type == 0)
            {
                return null;
            }

            LoadProcedureHandler handler = _loadProcedureHandlerDict[type];
            return handler;
        }

        public void ChangeProcedure(int type)
        {
            if (type == 0)
            {
                return;
            }

            LoadProcedureHandler handler = _loadProcedureHandlerDict[type];
            if (handler == null)
            {
                return;
            }

            ChangeProcedure(handler);
        }

        private void ChangeProcedure(LoadProcedureHandler handler)
        {
            if (handler == null)
                return;

            if (_curProcedureHandler != null && _curProcedureHandler.procedureType == handler.procedureType)
                return;

            if (!handler.ProcedureEnterStateHandler())
                return;

            LoadProcedureHandler last = null;
            if (_curProcedureHandler != null)
            {
                last = _curProcedureHandler;
            }

            _curProcedureHandler = handler;

            Log.Info($"ChangeProcedure： procedure type === {_curProcedureHandler.procedureType.ToString()} loading type ==== {_curProcedureHandler.loadType.ToString()}");

            _procedureHelper.ResetSpeed();

            _inLoading = false;
            handler.IsLoading = true;
            handler.IsCleanup = false;
            handler.startLoadTime = Time.realtimeSinceStartup;
            handler.ProcedureLoadHandler();

            Log.Info($"BeginLoad： procedure type === {_curProcedureHandler.procedureType.ToString()} loading type ==== {_curProcedureHandler.loadType.ToString()}");
            this.StartCoroutine(UnloadProcedureCoroutine(last));
        }

        private IEnumerator UnloadProcedureCoroutine(LoadProcedureHandler last)
        {
            if (_curProcedureHandler == null)
                yield break;

            //移除旧的
            if (last != null)
            {
                last.ProcedureExitHandler();
                last.IsCleanup = true;
                yield return null;
            }

            _procedureHelper.UnloadAllPanel(last, _curProcedureHandler);

            yield return null;

            GC.Collect();

            yield return null;

            Log.Info($"UnloadProcedureCoroutine： procedure type === {_curProcedureHandler.procedureType.ToString()} loading type ==== {((LoadingType)_curProcedureHandler.loadType).ToString()}");
            _curProcedureHandler.ProcedureStartHandler();
        }

        public void CleanProcedure()
        {
            if (_curProcedureHandler != null)
            {
                _curProcedureHandler.ProcedureExitHandler();
                _curProcedureHandler = null;
            }
        }

        #region 切流程界面

        public void OpenChangeProcedurePanel()
        {
            _procedureHelper.OpenChangeProcedurePanel(_curProcedureHandler);
        }

        public void CleanChangeProcedureParam()
        {
            if (_curProcedureHandler != null)
                _curProcedureHandler.turnNode = null;
        }

        #endregion
    }
}
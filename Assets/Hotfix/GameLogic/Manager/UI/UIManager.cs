using UnityEngine;
using System.Collections;
using System;
using LccModel;

namespace LccHotfix
{
    public partial class UIManager : Module, IUIService
    {
        private GameObject _uiRoot;
        private Transform _windowRoot;
        private GameObject _uiMask;
        private Camera _uiCamera;
        private IUIHelper _uiHelper;

        public Camera UICamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    if (_uiRoot != null)
                    {
                        _uiCamera = _uiRoot.GetComponentInChildren<Camera>();
                    }
                }

                return _uiCamera;
            }
        }

        public UIManager()
        {
            _uiRoot = GameObject.Find("UI Root");
            if (_uiRoot != null)
            {
                _windowRoot = ClientTools.GetChild(_uiRoot, "WindowRoot").transform;
            }

            _uiMask = ClientTools.GetChild(_uiRoot, "WindowRoot/UIMask/image");
            _uiMask.SetActive(false);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {

        }

        public void SetUIHelper(IUIHelper uiHelper)
        {
            _uiHelper = uiHelper;
        }

        public void Init()
        {
            Main.WindowService.UICamera = UICamera;
            Main.WindowService.WindowRoot = _windowRoot;
            Main.WindowService.GetModeFunc = GetWindowMode;



            Main.WindowService.LoadAsyncGameObject = (loader, asset, end) => { loader.LoadAssetAsync<GameObject>(asset, handle => { end?.Invoke(handle.AssetObject as GameObject); }); };
            Main.WindowService.InitializeForAssembly(Launcher.Instance.HotfixAssembly);
            Main.WindowService.Init();
        }



        public WindowMode GetWindowMode(string windowName)
        {
            return _uiHelper.GetWindowMode(windowName);
        }









        #region 打开面板

        public void OpenPanel(string panelID, params object[] paramsList)
        {
            Main.WindowService.OpenWindow(panelID, paramsList);
        }


        /// <summary>
        /// 延迟打开界面
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="time"></param>
        /// <param name="onFinish"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerator OpenPanelDelay(string panelName, float time, Action onFinish, params object[] args)
        {
            if (time <= 0)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(time);
            }

            OpenPanel(panelName, args);
            if (onFinish != null)
            {
                onFinish();
            }
        }

        #endregion


        #region 关闭面板

        public object ClosePanel(string panelID)
        {
            return Main.WindowService.CloseWindow(panelID);
        }


        /// <summary>
        /// 顶部栏和返回键调用，关闭当前ui
        /// </summary>
        /// <param name="withCloseApp">最后一个ui时调用，是否弹关闭游戏提示</param>
        public void CloseTopPanel(bool withCloseApp)
        {
            Main.WindowService.EscapeTopWindow();
        }

        #endregion

        #region 查询方法

        public UILogicBase GetPanel(string panelID)
        {
            Window window = Main.WindowService.GetWindow(panelID);
            if (window != null)
            {
                return window.Logic as UILogicBase;
            }

            return null;
        }

        public IUILogic GetPanelLogic(string panelID)
        {
            Window window = Main.WindowService.GetWindow(panelID);
            if (window != null)
            {
                return window.Logic;
            }

            return null;
        }

        public bool IsPanelActive(string panelID)
        {
            Window window = Main.WindowService.GetWindow(panelID);
            if (window != null)
            {
                return window.Active;
            }

            return false;
        }


        public string GetTopPanelID()
        {
            var window = Main.WindowService.GetTopWindow();
            if (window != null) return window.NodeName;
            return string.Empty;
        }

        public UILogicBase GetTopPanel()
        {
            var window = Main.WindowService.GetTopWindow();
            if (window != null) return window.Logic as UILogicBase;
            return null;
        }

        #endregion


    }
}
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
            Main.WindowService.GetUILogicMonoFunc = GetMonoLogic;
            Main.WindowService.EscapeJudgeFunc = IsEscapeEnable;
            Main.WindowService.SortDepthFunc = BuildPanelDepth;
            Main.WindowService.PauseWindowFunc = (transform, active) => transform.gameObject.SetActive(active); //MUtils.SetActive;
            Main.WindowService.RefreshBackgroundFunc = RefreshBg;
            Main.WindowService.PlayWindowSoundFunc = null; //AudioMgr.Instance.PlayAudio;
            Main.WindowService.ShowScreenMaskFunc = ShowScreenMask;
            Main.WindowService.ShowMaskBoxFunc = ShowMask;
            Main.WindowService.GetMaskBoxStateFunc = GetMaskState;
            Main.WindowService.ShowNoticeFunc = ShowNotice;
            Main.WindowService.ShowSelectFunc = ShowSelect;
            Main.WindowService.LoadGameObject = (asset, keepHierar) => Main.AssetService.LoadGameObject(asset, keepHierar);
            Main.WindowService.InitializeForAssembly(Launcher.Instance.hotfixAssembly);
        }

        public IUILogic GetMonoLogic(Window window, Type monoType)
        {
            if (window.gameObject == null) return null;
            IUILogic panel = window.gameObject.GetComponent<IUILogic>();
            if (panel == null)
            {
                panel = window.gameObject.AddComponent(monoType) as IUILogic;
            }

            return panel;
        }

        public WindowMode GetWindowMode(string windowName)
        {
            return _uiHelper.GetWindowMode(windowName);
        }

        public bool IsEscapeEnable()
        {
            if (maskState == 0)
            {
                return true;
            }

            return false;
        }

        private void RefreshBg(Window window, string bg)
        {
        }

        private void ShowScreenMask()
        {
            UIForeGroundPanel.Instance.FadeOut(0.5f);
        }

        private void ShowNotice(string msg, Action sure)
        {
        }

        private void ShowSelect(string msg, Action sure, Action cancel)
        {
        }

        #region 打开面板

        public UILogicBase OpenPanel(string panelID, params object[] paramsList)
        {
            var window = UI.OpenWindow(panelID, paramsList);
            if (window != null) return window.Logic as UILogicBase;
            return null;
        }

        public void BuildPanelDepth(GameObject obj, int depth)
        {
            if (obj == null)
            {
                return;
            }
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
            ShowMask((int)MaskType.WINDOW_ANIM, true);
            if (time <= 0)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(time);
            }

            OpenPanel(panelName, args);
            ShowMask((int)MaskType.WINDOW_ANIM, false);
            if (onFinish != null)
            {
                onFinish();
            }
        }

        #endregion


        #region 关闭面板

        public void ClosePanel(int rejectFlag)
        {
            UI.Close(rejectFlag);
        }

        public object ClosePanel(string panelID)
        {
            return UI.Close(panelID);
        }


        /// <summary>
        /// 顶部栏和返回键调用，关闭当前ui
        /// </summary>
        /// <param name="withCloseApp">最后一个ui时调用，是否弹关闭游戏提示</param>
        public void CloseTopPanel(bool withCloseApp)
        {
            UI.CloseTop();
        }

        #endregion

        #region 查询方法

        public UILogicBase GetPanel(string panelID)
        {
            Window window = UI.GetWindow(panelID);
            if (window != null)
            {
                return window.Logic as UILogicBase;
            }

            return null;
        }

        public IUILogic GetPanelLogic(string panelID)
        {
            Window window = UI.GetWindow(panelID);
            if (window != null)
            {
                return window.Logic;
            }

            return null;
        }

        public bool IsPanelActive(string panelID)
        {
            Window window = UI.GetWindow(panelID);
            if (window != null)
            {
                return window.Active;
            }

            return false;
        }


        public string GetTopPanelID()
        {
            var window = UI.GetTopWindow();
            if (window != null) return window.NodeName;
            return string.Empty;
        }

        public UILogicBase GetTopPanel()
        {
            var window = UI.GetTopWindow();
            if (window != null) return window.Logic as UILogicBase;
            return null;
        }

        #endregion


        #region UI辅助管理方法

        private int maskState = 0;

        /// <summary>
        /// 显示一个全屏遮罩，用于业务处理
        /// </summary>
        /// <param name="state"></param>
        public void ShowMask(int maskType, bool enable)
        {
            if (enable)
            {
                maskState |= maskType;
            }
            else
            {
                maskState &= (maskType ^ -1);
            }

            _uiMask.SetActive(maskState > 0);
        }

        public bool GetMaskState()
        {
            return maskState > 0;
        }

        #endregion
    }
}
using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using LccModel;

namespace LccHotfix
{
    internal partial class WindowManager : Module
    {
        public static WindowManager Instance => Entry.GetModule<WindowManager>();
        public GameObject uiRoot;

        private GameObject _uiMask;

        private UIWindowModeDefine _uiWindowModeDefine;
        public void InitWindowManager()
        {
            uiRoot = GameObject.Find("UI Root");

            var uiCamera = uiRoot.GetComponentInChildren<Camera>();

            var windowRoot = uiRoot.transform.Find("WindowRoot").transform;

            _uiMask = uiRoot.transform.Find("WindowRoot/UIMask").gameObject;
            _uiMask.SetActive(false);

            _uiWindowModeDefine = new UIWindowModeDefine();

            UICamera = uiCamera;
            WindowRoot = windowRoot;
            GetModeFunc = GetWindowMode;
            GetUILogicMonoFunc = GetUILogic;
            EscapeJudgeFunc = IsEscapeEnable;
            SortDepthFunc = null;
            PauseWindowFunc = (transform, active) => transform.gameObject.SetActive(active);
            RefreshBackgroundFunc = null;
            PlayWindowSoundFunc = null;
            ShowScreenMaskFunc = () => UIForeGroundPanel.Instance.FadeOut(0.5f);
            ShowMaskBoxFunc = ShowMask;
            GetMaskBoxStateFunc = GetMaskState;
            ShowNoticeFunc = null;
            ShowSelectFunc = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var asName = assembly.GetName();
                if (asName.Name == "Unity.Model")
                {
                    InitializeForAssembly(assembly);
                    break;
                }
            }

            Init();
        }

        public IUILogic GetUILogic(Window window, Type monoType)
        {
            if (window.gameObject == null) return null;
            IUILogic logic = window.gameObject.GetComponent<IUILogic>();
            if (logic == null)
            {
                logic = window.gameObject.AddComponent(monoType) as IUILogic;
            }

            return logic;
        }

        public WindowMode GetWindowMode(string windowName)
        {
            var windowMode = _uiWindowModeDefine.Get(windowName);
            return windowMode;
        }

        public bool IsEscapeEnable()
        {
            if (maskState == 0)
            {
                return true;
            }

            return false;
        }

        #region 打开面板

        public T OpenWindow<T>(string windowName, params object[] paramsList) where T : UILogicBase
        {
            var window = OpenWindow(windowName, paramsList);
            if (window != null)
                return window.Logic as T;
            return null;
        }



        /// <summary>
        /// 延迟打开界面
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="time"></param>
        /// <param name="onFinish"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerator OpenWindowDelay(string windowName, float time, Action onFinish, params object[] args)
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

            OpenWindow(windowName, args);
            ShowMask((int)MaskType.WINDOW_ANIM, false);
            if (onFinish != null)
            {
                onFinish();
            }
        }

        #endregion




        #region 查询方法

        public T GetUILogic<T>(string windowName) where T : UILogicBase
        {
            Window window = GetWindow(windowName);
            if (window != null)
            {
                return window.Logic as T;
            }

            return null;
        }


        public bool IsWindowActive(string windowName)
        {
            Window window = GetWindow(windowName);
            if (window != null)
            {
                return window.Active;
            }

            return false;
        }

        public string GetTopWindowName()
        {
            var window = GetTopWindow();
            if (window != null)
                return window.NodeName;
            return string.Empty;
        }
        public T GetTopWindow<T>() where T : UILogicBase
        {
            var window = GetTopWindow();
            if (window != null)
                return window.Logic as T;
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
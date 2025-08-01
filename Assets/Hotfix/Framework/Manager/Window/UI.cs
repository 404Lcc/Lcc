using System;

namespace LccHotfix
{
    public static class UI
    {
        public static Window OpenWindow(string windowName, object[] param = null)
        {
            return Main.WindowService.OpenWindow(windowName, param);
        }

        public static WRootNode OpenRoot(string rootName, object[] param = null)
        {
            return Main.WindowService.OpenRoot(rootName, param);
        }

        public static Window OpenChild(this IUILogic logic, string windowName, object[] param = null)
        {
            return Main.WindowService.OpenWindow(logic.WNode, windowName, param);
        }

        public static Window OpenChild(this WNode openBy, string windowName, object[] param = null)
        {
            return Main.WindowService.OpenWindow(openBy, windowName, param);
        }

        public static object Close(string windowName)
        {
            return Main.WindowService.CloseWindow(windowName);
        }

        public static void Close(int windowFlag)
        {
            Main.WindowService.CloseWindow(windowFlag);
        }

        public static void CloseTop()
        {
            Main.WindowService.EscapeTopWindow();
        }

        public static void CloseAll()
        {
            Main.WindowService.CloseAllWindow();
        }

        public static void Release(ReleaseType level)
        {
            Main.WindowService.ReleaseAllWindow(level);
        }

        public static Window GetWindow(string windowName)
        {
            return Main.WindowService.GetWindow(windowName);
        }

        public static WRootNode GetTopRoot()
        {
            return Main.WindowService.GetTopRoot();
        }

        public static WRootNode GetCommonRoot()
        {
            return Main.WindowService.CommonRoot;
        }

        public static WRootNode GetRoot(string rootName)
        {
            return Main.WindowService.GetRoot(rootName);
        }

        public static Window GetTopWindow()
        {
            return Main.WindowService.GetTopWindow();
        }

        public static void AddCloseCallback(string windowName, Action<object> callback)
        {
            Main.WindowService.AddCloseCallback(windowName, callback);
        }

        public static void RemoveCloseCallback(string windowName, Action<object> callback)
        {
            Main.WindowService.RemoveCloseCallback(windowName, callback);
        }

        public static void PlayWindowSound(int soundId)
        {
            Main.WindowService.PlayWindowSoundFunc?.Invoke(soundId);
        }

        public static void ShowMaskBox(int maskType, bool enable)
        {
            Main.WindowService.ShowMaskBoxFunc?.Invoke(maskType, enable);
        }

        public static void ShowScreenFadeMask()
        {
            Main.WindowService.ShowScreenMaskFunc?.Invoke();
        }

        public static void ShowSelect(string msg, Action sure, Action cancel = null)
        {
            Main.WindowService.ShowSelectFunc?.Invoke(msg, sure, cancel);
        }

        public static void ShowNotice(string msg, Action sure)
        {
            Main.WindowService.ShowNoticeFunc?.Invoke(msg, sure);
        }

        public static void ClosedLastRoot()
        {
            Main.WindowService.OnClosedLastRootFunc?.Invoke();
        }
    }
}
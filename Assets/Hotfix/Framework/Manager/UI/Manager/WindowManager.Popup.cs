using System.Collections.Generic;
using System.Linq;
using cfg;

namespace LccHotfix
{
	public enum WindowType
	{
		Normal,
		Advance,
		Special,
	}
	public class PopupWindowData
	{
		public WindowType type;
		public string windowName;
		public int sortValue;
		public object[] param;
		public PopupWindowData(PopupWindow popup, object[] param)
		{
			this.windowName = popup.WindowName;
			this.type = (WindowType)popup.PopType;
			this.sortValue = popup.SortValue;
			this.param = param;
		}
	}

	internal partial class WindowManager : Module
	{

		private List<PopupWindowData> _popList = new List<PopupWindowData>();

        /// <summary>
        /// 当前普通弹窗
        /// </summary>
        private string _normalWindow = null;
        /// <summary>
        /// 当前高级弹窗
        /// </summary>
        private string _advanceWindow = null;


		/// <summary>
		/// 是否自动弹出礼包
		/// </summary>
		public bool isAutoPop = true;

        private List<string> _preventPopupWindow = new List<string>();

		bool HaveAnyPreventPopupWindowOpened()
		{
			foreach (var w in _preventPopupWindow)
			{
				if (IsWindowActive(w))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 保存弹窗数据，在主城会按顺序弹出 最多支持三个参数
		/// </summary>
		/// <param name="type">面板类型</param>
		/// <param name="param">面板参数</param>
		public void StackWindow(string windowName, object[] param)
		{
			PopupWindow popup = ConfigManager.Instance.Tables.TBPopupWindow.Get(windowName);
			if (popup != null)
			{
				_popList.Add(new PopupWindowData(popup, param));
			}

			_popList = _popList.OrderBy(data => (int)data.sortValue).ToList();

			if (isAutoPop)
				TryPopupWindow();
		}
		public void StackWindow(string windowName)
		{
			StackWindow(windowName, null);
		}
		public void StackWindow(string windowName, object param)
		{
			StackWindow(windowName, new object[] { param });
		}
		/// <summary>
		/// 判断是否压入了某种类型的弹窗
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool IsStackWindow(string windowName)
		{
			if (_popList == null)
				return false;

			for (int i = 0; i < _popList.Count; i++)
			{
				if (_popList[i].windowName == windowName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 打开弹窗
		/// </summary>
		public void TryPopupWindow()
		{
			if (_popList.Count == 0)
			{
				return;
			}

			if (HaveAnyPreventPopupWindowOpened())
			{
				return;
			}

			if (!CheckScene() || SceneManager.Instance.IsLoading)
			{
				return;
			}

			PopupWindowData popData = _popList[0];
			bool isAdvance = popData.type > WindowType.Normal;

			//todo判断新手引导

			if (isAdvance)
			{
				if (_normalWindow != null || _advanceWindow != null) return;
			}
			else
			{
				if (_normalWindow != null) return;
			}

			_popList.RemoveAt(0);
			string popWindowName = popData.windowName;

			var currentWindow = OpenWindow(popWindowName, popData.param);
			if (currentWindow == null)
			{
				TryPopupWindow();
			}
			else if (currentWindow.Active)
			{
				if (isAdvance)
					_advanceWindow = popWindowName;
				else
					_normalWindow = popWindowName;

				AddCloseCallback(popWindowName, (o) => OnPopWindowClose(popWindowName));
			}
		}

		private bool CheckScene()
		{
			return SceneManager.Instance.curState == SceneType.Main;
		}


		public void OnPopWindowClose(string windowName)
		{
			RemoveCloseCallback(windowName, (o) => OnPopWindowClose(windowName));
			if (windowName == _normalWindow)
				_normalWindow = null;
			else if (windowName == _advanceWindow)
				_advanceWindow = null;
			else
				return;
			TryPopupWindow();
		}
	}
}
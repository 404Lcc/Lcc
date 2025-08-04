using System.Collections.Generic;
using System.Linq;
using cfg;

namespace LccHotfix
{
	public enum PopupPanelType
	{
		Normal,
		Advance,
		Special,
	}
	public class PopupPanelData
	{
		public PopupPanelType type;
		public string panelName;
		public int sortValue;
		public object[] param;
		public PopupPanelData(PopupPanel popup, object[] param)
		{
			this.panelName = popup.PanelName;
			this.type = (PopupPanelType)popup.PopType;
			this.sortValue = popup.SortValue;
			this.param = param;
		}
	}

	public partial class UIManager : Module
	{
		private List<PopupPanelData> _popList = new List<PopupPanelData>();

        /// <summary>
        /// 当前普通弹窗
        /// </summary>
        private string _normalPanel = null;
        /// <summary>
        /// 当前高级弹窗
        /// </summary>
        private string _advancePanel = null;


		/// <summary>
		/// 是否自动弹出礼包
		/// </summary>
		public bool isAutoPop = true;

        private List<string> _preventPopupPanel = new List<string>();

		bool HaveAnyPreventPopupPanelOpened()
		{
			foreach (var w in _preventPopupPanel)
			{
				if (Main.UIService.IsPanelActive(w))
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
		public void StackPanel(string panelName, object[] param)
		{
			PopupPanel popup = Main.ConfigService.Tables.TBPopupPanel.Get(panelName);
			if (popup != null)
			{
				_popList.Add(new PopupPanelData(popup, param));
			}

			_popList = _popList.OrderBy(data => (int)data.sortValue).ToList();

			if (isAutoPop)
				TryPopupPanel();
		}
		public void StackPanel(string panelName)
		{
			StackPanel(panelName, null);
		}
		public void StackPanel(string panelName, object param)
		{
			StackPanel(panelName, new object[] { param });
		}
		/// <summary>
		/// 判断是否压入了某种类型的弹窗
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool IsStackPanel(string panelName)
		{
			if (_popList == null)
				return false;

			for (int i = 0; i < _popList.Count; i++)
			{
				if (_popList[i].panelName == panelName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 打开弹窗
		/// </summary>
		public void TryPopupPanel()
		{
			if (_popList.Count == 0)
			{
				return;
			}

			if (HaveAnyPreventPopupPanelOpened())
			{
				return;
			}

			if (!CheckProcedure() || Main.ProcedureService.IsLoading)
			{
				return;
			}

			PopupPanelData popData = _popList[0];
			bool isAdvance = popData.type > PopupPanelType.Normal;

			//todo判断新手引导

			if (isAdvance)
			{
				if (_normalPanel != null || _advancePanel != null) return;
			}
			else
			{
				if (_normalPanel != null) return;
			}

			_popList.RemoveAt(0);
			string popPanelName = popData.panelName;

			var currentPanel = UI.OpenWindow(popPanelName, popData.param);
			if (currentPanel == null)
			{
				TryPopupPanel();
			}
			else if (currentPanel.Active)
			{
				if (isAdvance)
					_advancePanel = popPanelName;
				else
					_normalPanel = popPanelName;

				UI.AddCloseCallback(popPanelName, (o) => OnPopPanelClose(popPanelName));
			}
		}

		private bool CheckProcedure()
		{
			return Main.ProcedureService.CurState == ProcedureType.Main;
		}


		public void OnPopPanelClose(string panelName)
		{
			UI.RemoveCloseCallback(panelName, (o) => OnPopPanelClose(panelName));
			if (panelName == _normalPanel)
				_normalPanel = null;
			else if (panelName == _advancePanel)
				_advancePanel = null;
			else
				return;
			TryPopupPanel();
		}
	}
}
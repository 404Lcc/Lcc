using System;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module
	{
		/// <summary>
		/// 从prefab上获取mono对象
		/// </summary>
		public Func<Window, Type, IUILogic> GetUILogicMonoFunc { get; set; }

		/// <summary>
		/// 从表里获取窗口的配置
		/// </summary>
		public Func<string, WindowMode> GetModeFunc { get; set; }
		/// <summary>
		/// 获取窗口的父节点
		/// </summary>
		public Transform WindowRoot { get; set; }
		/// <summary>
		/// ui相机
		/// </summary>
		public Camera UICamera { get; set; }
		/// <summary>
		/// 深度排序方法
		/// </summary>
		public Action<GameObject, int> SortDepthFunc { get; set; }
		/// <summary>
		/// 获取当前返回键是否生效
		/// </summary>
		public Func<bool> EscapeJudgeFunc { get; set; }
		/// <summary>
		/// 暂停一个window的方式
		/// </summary>
		public Action<Transform, bool> PauseWindowFunc { get; set; }
		/// <summary>
		/// 刷新背景图
		/// </summary>
		public Action<Window, string> RefreshBackgroundFunc { get; set; }
		/// <summary>
		/// 播放界面音效
		/// </summary>
		public Action<int> PlayWindowSoundFunc { get; set; }
		/// <summary>
		/// 确认弹窗
		/// </summary>
		public Action<string, Action> ShowNoticeFunc { get; set; }
		/// <summary>
		/// 选择弹窗
		/// </summary>
		public Action<string, Action, Action> ShowSelectFunc { get; set; }
		/// <summary>
		/// 显示屏幕遮罩
		/// </summary>
		/// <returns></returns>
		public Action ShowScreenMaskFunc { get; set; }

		/// <summary>
		/// 显示屏幕遮挡碰撞框
		/// </summary>
		/// <returns></returns>
		public Action<int, bool> ShowMaskBoxFunc { get; set; }
		/// <summary>
		/// 获取屏幕遮挡状态
		/// </summary>
		public Func<bool> GetMaskBoxStateFunc { get; set; }
		/// <summary>
		/// 关闭最后一个root
		/// </summary>
		public Action OnClosedLastRootFunc { get; set; }
	}
}
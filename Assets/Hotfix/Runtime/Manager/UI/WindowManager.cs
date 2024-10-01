using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module
	{
		/// <summary>
		/// 当前活动窗口的栈
		/// 栈里的每个窗口实际是一个全屏窗口和从属于这个全屏窗口的子窗口
		/// 每个窗口的作用域是自己和从属于自己的子窗口，不能跨域修改其它窗口
		/// </summary>
		private Stack<WRootNode> m_rootStack = new Stack<WRootNode>();
		/// <summary>
		/// 当前活动的通用窗口
		/// 这些特殊窗口不受栈的限制，可以用任意方式唤醒和关闭
		/// </summary>
		private WRootNode m_commonRoot;
		public WRootNode commonRoot => m_commonRoot;
		/// <summary>
		/// 等待释放的窗口
		/// </summary>
		private List<WNode> m_waitReleaseWindow = new List<WNode>();
		/// <summary>
		/// 缓存窗口配置
		/// </summary>
		private Dictionary<string, WindowMode> m_windowModeDic = new Dictionary<string, WindowMode>();
		/// <summary>
		/// 被关闭界面会自动缓存多少帧然后释放
		/// 30s
		/// </summary>
		public int autoCacheTime = 900;
		/// <summary>
		/// 窗口父节点
		/// </summary>
		internal Transform windowRoot { get { return TDUI.WindowRoot; } }
		/// <summary>
		/// 窗口关闭回调
		/// </summary>
		private Dictionary<string, Action<object>> m_windowCloseCallback = new Dictionary<string, Action<object>>();
		

		public bool Enable => true;
		public string Name => "WindowManager";

		private WNode switchingNode;

		internal void Init()
		{
			m_commonRoot = GetAndCreateRoot("UIRootCommon");
			m_commonRoot.stackIndex = 0;
			m_commonRoot.Open(null);
			m_commonRoot.Resume();
		}

		List<WNode> m_updateNodes = new List<WNode>();

		internal override void Update(float elapseSeconds, float realElapseSeconds)
		{
			if (m_commonRoot != null)
			{
				m_updateNodes.Clear();
				m_commonRoot.GetAllChild(m_updateNodes);
				foreach(var node in m_updateNodes)
                {
					node.Update();
                }
			}

			if (m_rootStack.Count == 0)
				return;
			var peekWindow = m_rootStack.Peek();
			if (peekWindow != null)
			{
				m_updateNodes.Clear();
				peekWindow.GetAllChild(m_updateNodes);
				foreach (var node in m_updateNodes)
				{
					node.Update();
				}
			}
		}

		private bool m_escaped = false;
		internal void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (!m_escaped)
				{
					bool escape = TDUI.EscapeJudgeFunc == null ? true : TDUI.EscapeJudgeFunc.Invoke();
					if (escape)
					{
						EscapeTopWindow();
					}
					m_escaped = true;
				}
				
			}
			else if(m_escaped)
			{
				m_escaped = false;
			}
			AutoReleaseWindow();
		}
		internal override void Shutdown()
		{
			CloseAllWindow();
			ReleaseAllWindow(ReleaseType.NEVER);
		}


		internal Window OpenWindow(WNode openBy, string windowName, object[] param)
		{
			if (switchingNode != null) 
			{
				Log.Error($"request open window {windowName} during switch one other window {switchingNode.nodeName}");
				return null;
			}
			if (openBy == null) 
				return OpenWindow(windowName, param);

			// 打开一个与自己同名的界面
            if (openBy.nodeName == windowName)
            {
				Log.Error($"request open a same name child window {windowName}");
				return null;
			}
			
			if (!openBy.TryGetNode(windowName, out WNode openedWindow))
			{
				if(!m_windowModeDic.TryGetValue(windowName, out WindowMode mode))
				{
					mode = TDUI.GetModeFunc.Invoke(windowName);
					m_windowModeDic.Add(windowName, mode);
				}
				
				openedWindow = CreateWindow(windowName, mode);
				openedWindow.RootNode = openBy.RootNode;
				openedWindow.transform.SetParent(openBy.RootNode.transform);
				openedWindow.transform.localPosition = Vector3.zero;
				openedWindow.transform.localRotation = Quaternion.identity;
				openedWindow.transform.localScale = Vector3.one;

			}
			SwitchWindow(openedWindow, openBy, param);

			return openedWindow as Window;
		}
		
		/// <summary>
		/// 打开一个界面
		/// 这里只是创建，并不会改变当前栈结构
		/// 确认界面可打开后才会继续
		/// </summary>
		/// <param name="windowName"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		internal Window OpenWindow(string windowName, object[] param)
		{
			if (switchingNode != null)
			{
				Log.Error($"request open window {windowName} during switch one other window {switchingNode.nodeName}");
				return null;
			}

			Log.Debug($"open window {windowName}");

			if (!m_windowModeDic.TryGetValue(windowName, out WindowMode mode))
			{
				mode = TDUI.GetModeFunc.Invoke(windowName);
				m_windowModeDic.Add(windowName, mode);
			}

			WRootNode root = GetAndCreateRoot(mode.rootName);

			WNode window = null;
			if (!root.TryGetNode(windowName, out window)) 
			{
				window = CreateWindow(windowName, mode);
				window.RootNode = root;
				window.transform.SetParent(root.transform);
				window.transform.localPosition = Vector3.zero;
				window.transform.localRotation = Quaternion.identity;
				window.transform.localScale = Vector3.one;
			}
			SwitchWindow(window, root, param);

			return window as Window;
		}

		internal WRootNode OpenRoot(string rootName, object[] param)
		{
			if (switchingNode != null)
			{
				Log.Error($"request open window {rootName} during switch one other window {switchingNode.nodeName}");
				return null;
			}

			if (string.IsNullOrEmpty(rootName)) return null;
			
			WRootNode root = GetAndCreateRoot(rootName);
			SwitchWindow(root, null, param);

			return root;
		}

		private void SwitchWindow(WNode window, WNode parentNode, object[] param)
		{
			switchingNode = window;
			ShowMaskBox((int)MaskType.WINDOW_SWITCH, true);
			window.Switch((canOpen) => SwitchEnd(window, parentNode, canOpen, param));
		}
		private void SwitchEnd(WNode window, WNode parentNode, bool canOpen, object[] param)
		{
			ShowMaskBox((int)MaskType.WINDOW_SWITCH, false);
			if (switchingNode == null) 
				return;
			if (switchingNode != window)
				return;
			switchingNode = null;
			// 打开失败
			if (!canOpen)
			{
				AddToReleaseQueue(window);
				return;
			}
			bool switchScreen = false;
			var root = window.RootNode;

			if (root != commonRoot)
			{
				if (root.stackIndex < 0)
				{
					switchScreen = true;
					if (m_rootStack.Count > 0)
						m_rootStack.Peek().Pause();
					root.stackIndex = m_rootStack.Count;
					m_rootStack.Push(root);
					if (window == root)
					{
						root.Open(param);
					}
					else
					{
						root.Open(null);
					}
					root.Resume();
				}
				else
				{
					bool isTop = m_rootStack.Count == window.RootNode.stackIndex + 1;
					if (!isTop)
					{
						switchScreen = true;
						while (m_rootStack.Peek() != root)
						{
							var top = m_rootStack.Pop();
							top.Close();
						}
					}
					if (window == root)
					{
						root.Reset(param);
					}
					root.Resume();
				}
			}
			

			if (window is Window)
			{
				if (root.Contains(window))
				{
					// 保持队列的顺序不变,不能循环打开
					if (window.ParentNode == root && (window.isFullScreen || window.isMainNode)) 
					{
						for (int i = root.childNode.Count - 1; i >= 0; i--)
						{
							if (root.childNode[i] == window)
								break;
							root.childNode[i].Close();
						}
					}
					window.ParentNode = parentNode;
					window.Reset(param);
					window.Resume();
				}
				else
                {
                    int mask = ((Window)window).windowMode.showScreenMask;
                    if (mask == 2)
                        switchScreen = true;
                    else if (mask == 1 && window.newCreate)
                        switchScreen = true;

                    window.ParentNode = parentNode;
                    window.Open(param);
					window.Resume();
				}
			}
			if (switchScreen)
			{
				ShowScreenMask();
			}
		}

		private WRootNode GetAndCreateRoot(string rootName)
        {
            if (string.IsNullOrEmpty(rootName))
            {
				Debug.Assert(m_rootStack.Count > 0);
				return m_rootStack.Peek();
            }

			if (m_commonRoot != null && rootName == m_commonRoot.nodeName) 
				return m_commonRoot;

			foreach(var item in m_rootStack)
            {
                if (item.nodeName.Equals(rootName))
                {
					return item;
                }
            }
			WRootNode root = null;
			for (int i = 0; i < m_waitReleaseWindow.Count; i++)
			{
				if (m_waitReleaseWindow[i].nodeName.Equals(rootName))
				{
					root = m_waitReleaseWindow[i] as WRootNode;
					m_waitReleaseWindow.RemoveAt(i);
					break;
				}
			}
			if (root == null)
			{
				root = new WRootNode(rootName);
				root.gameObject = new GameObject(rootName);
				root.transform = root.gameObject.transform;
				root.Start();
			}

			root.transform.SetParent(windowRoot);
			root.transform.localPosition = Vector3.zero;
			root.transform.localScale = Vector3.one;
			root.transform.localRotation = Quaternion.identity;
			root.stackIndex = -1;
			return root;
        }


		private Window CreateWindow(string windowName, WindowMode mode)
		{
			Window window = null;
			for (int i = 0; i < m_waitReleaseWindow.Count; i++) 
			{
				if (m_waitReleaseWindow[i].nodeName.Equals(windowName)) 
				{
					window = m_waitReleaseWindow[i] as Window;
					m_waitReleaseWindow.RemoveAt(i);
					break;
				}
			}
			if (window == null)
			{
				window = new Window(windowName, mode);
				window.CreateWindowView();
				TDUI.SortDepthFunc?.Invoke(window.gameObject, mode.depth);
				CreateUILogic(window);
				window.Start();
			}
			return window;
		}


		/// <summary>
		/// 关闭一个窗口
		/// window是有作用域的
		/// 通过这个方法默认是关闭一个栈内的全屏窗口或当前栈顶窗口的子窗口
		/// 不能用来关闭一个不活跃窗口的子窗口
		/// </summary>
		/// <param name="windowClose"></param>
		/// <returns></returns>
		internal object CloseWindow(string windowClose)
		{
			if (string.IsNullOrEmpty(windowClose)) return null;
			if(m_commonRoot.TryCloseChild(windowClose,out object val))
			{
				return val;
			}

			if (m_rootStack.Count == 0) return null;

			Log.Debug($"close window {windowClose}");
			
			WRootNode root = m_rootStack.Peek();

			// 关闭子窗口
			if(root.TryCloseChild(windowClose, out val))
            {
				return val;
            }
			return null;
		}


		internal void CloseWindow(int windowFlag)
		{
			if (m_rootStack.Count == 0) return;
			if (0 == windowFlag) return;

			WRootNode root = m_rootStack.Peek();
			root.CloseChild(windowFlag);
		}


		internal void CloseAllWindow()
		{
			if (switchingNode != null)
			{
				AddToReleaseQueue(switchingNode);
				switchingNode = null;
			}
			commonRoot.CloseAllChild();
			while (m_rootStack.Count > 0) 
			{
				m_rootStack.Pop().Close();
			}
		}

		internal void EscapeTopWindow()
		{
			EscapeType escape = EscapeType.AUTO_CLOSE;
			if (m_commonRoot.Escape(ref escape))
				return;

			if (m_rootStack.Count == 0) return;

			var top = m_rootStack.Peek();
			top.Escape(ref escape);
		}

	
		/// <summary>
		/// 关闭root时从栈内移除
		/// </summary>
		/// <param name="root"></param>
		internal void RemoveRoot(WRootNode root)
        {
			if (m_rootStack.Count == 0) return;

			if (root == m_rootStack.Peek())
			{
				m_rootStack.Pop();

				if (m_rootStack.Count > 0)
					m_rootStack.Peek().Resume();
			}
			else
			{
				var index = 0;
				foreach (var node in m_rootStack)
				{
					if (node == root)
						break;
					index++;
				}
				if (index == m_rootStack.Count)
					return;
				Stack<WRootNode> tempStack = new Stack<WRootNode>();

				while (index >= 0)
				{
					var top = m_rootStack.Pop();
					if (index-- == 0)
					{

					}
					else
						tempStack.Push(top);
				}

				while (tempStack.Count > 0)
				{
					var temp = tempStack.Pop();
					temp.stackIndex = m_rootStack.Count;
					m_rootStack.Push(temp);
				}

			}
			if (m_rootStack.Count == 0)
			{
				TDUI.ClosedLastRoot();
			}
		}

		private Transform releaseRoot;
		internal void AddToReleaseQueue(WNode node)
		{
			if (node != null)
			{
				if (node.releaseType == ReleaseType.AUTO)
				{
					node.releaseTimer = autoCacheTime;
				}
				else
				{
					node.releaseTimer = 0;
				}
				if (releaseRoot == null)
				{
					releaseRoot = (new GameObject("WaitForRelease")).transform;
					releaseRoot.SetParent(TDUI.WindowRoot);
					releaseRoot.localScale = Vector3.one;
					releaseRoot.localPosition = new Vector3(30000, 0, 0);
				}

				if(node.transform!=null)
				{
					node.transform.parent = releaseRoot;
				}	

				m_waitReleaseWindow.Add(node);
			}
		}


		private void AutoReleaseWindow()
		{
			for (int i = m_waitReleaseWindow.Count - 1; i >= 0; i--)  
			{
				if (m_waitReleaseWindow[i].AutoRemove())
				{
					m_waitReleaseWindow.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// 释放全部window资源
		/// </summary>
		/// <param name="type">筛选释放window的级别，释放小于这个级别的所有窗口</param>
		internal void ReleaseAllWindow(ReleaseType level = ReleaseType.AUTO)
		{
			for (int i = m_waitReleaseWindow.Count - 1; i >= 0; i--) 
			{
				if (m_waitReleaseWindow[i].releaseType<=level)
				{
					m_waitReleaseWindow[i].Remove();
					m_waitReleaseWindow.RemoveAt(i);
				}
			}
		}

		internal void AddCloseCallback(string windowName, Action<object> callback)
		{
			if (m_windowCloseCallback.TryGetValue(windowName, out Action<object> action))
			{
				action -= callback;
				action += callback;
			}
			else
			{
				m_windowCloseCallback.Add(windowName, callback);
			}
		}

		internal void RemoveCloseCallback(string windowName, Action<object> callback)
		{
			if (m_windowCloseCallback.TryGetValue(windowName, out Action<object> action))
			{
				action -= callback;
			}
		}

		internal void OnWindowClose(string windowName, object backValue)
		{
			if (m_windowCloseCallback.TryGetValue(windowName, out Action<object> action))
			{
				action?.Invoke(backValue);
			}
		}

		internal Window GetWindow(string windowName)
		{
			if (m_rootStack.Count == 0) return null;

			var top = m_rootStack.Peek();
			if(top.TryGetNode(windowName, out var node))
            {
				return node as Window;
			}
			
			return null;
		}

		internal WRootNode GetRoot(string rootName)
		{
			if (m_rootStack.Count == 0) return null;

			foreach(var root in m_rootStack)
            {
				if (root.nodeName == rootName)
					return root;
            }

			return null;
		}

		internal WRootNode GetTopRoot()
		{
			if (m_rootStack.Count == 0) return null;

			return m_rootStack.Peek();
		}

		internal Window GetTopWindow()
		{
			if (m_rootStack.Count == 0) return null;

			var peekWindow = m_rootStack.Peek();
			if (peekWindow != null)
				return peekWindow.GetTopWindow() as Window;
			return null;
		}

		/// <summary>
		/// 显示一个碰撞框，不能再次点击
		/// </summary>
		/// <param name="maskType"></param>
		internal void ShowMaskBox(int maskType, bool enable)
		{
			TDUI.ShowMaskBox(maskType, enable);
		}

		/// <summary>
		/// 屏幕遮黑淡入
		/// 替换以前的截屏操作，这个更快，不需要等待一帧
		/// </summary>
		internal void ShowScreenMask()
		{
			TDUI.ShowScreenFadeMask();
		}

		
	}
}

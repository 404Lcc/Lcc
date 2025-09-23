using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module, IWindowService
	{
		/// <summary>
		/// 当前活动窗口的栈
		/// 栈里的每个窗口实际是一个全屏窗口和从属于这个全屏窗口的子窗口
		/// 每个窗口的作用域是自己和从属于自己的子窗口，不能跨域修改其它窗口
		/// </summary>
		private Stack<WRootNode> _rootStack = new Stack<WRootNode>();
		/// <summary>
		/// 当前活动的通用窗口
		/// 这些特殊窗口不受栈的限制，可以用任意方式唤醒和关闭
		/// </summary>
		private WRootNode _commonRoot;
		public WRootNode CommonRoot => _commonRoot;
		/// <summary>
		/// 等待释放的窗口
		/// </summary>
		private List<WNode> _waitReleaseWindow = new List<WNode>();
		/// <summary>
		/// 缓存窗口配置
		/// </summary>
		private Dictionary<string, WindowMode> _windowModeDic = new Dictionary<string, WindowMode>();
		/// <summary>
		/// 被关闭界面会自动缓存多少帧然后释放
		/// 30s
		/// </summary>
		public int autoCacheTime = 900;
		/// <summary>
		/// 窗口父节点
		/// </summary>
		//internal Transform WindowRoot { get { return TDUI.WindowRoot; } }
		/// <summary>
		/// 窗口关闭回调
		/// </summary>
		private Dictionary<string, Action<object>> _windowCloseCallback = new Dictionary<string, Action<object>>();

		//当前切换中的窗口
		private WNode _switchingNode;
		//初始化通用节点
		public void Init()
		{
			_commonRoot = GetAndCreateRoot("UIRootCommon");
			_commonRoot.stackIndex = 0;
			_commonRoot.Open(null);
			_commonRoot.Resume();
		}
		//需要更新的节点列表
		List<WNode> _updateNodes = new List<WNode>();

		internal override void Update(float elapseSeconds, float realElapseSeconds)
		{
			if (_commonRoot != null)
			{
				_updateNodes.Clear();
				_commonRoot.GetAllChild(_updateNodes);
				foreach (var node in _updateNodes)
				{
					node.Update();
				}
			}

			if (_rootStack.Count == 0)
				return;
			var peekWindow = _rootStack.Peek();
			if (peekWindow != null)
			{
				_updateNodes.Clear();
				peekWindow.GetAllChild(_updateNodes);
				foreach (var node in _updateNodes)
				{
					node.Update();
				}
			}
		}
		//是否可以回退值
		private bool _escaped = false;
		internal override void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (!_escaped)
				{
					bool escape = EscapeJudgeFunc == null ? true : EscapeJudgeFunc.Invoke();
					if (escape)
					{
						EscapeTopWindow();
					}
					_escaped = true;
				}

			}
			else if (_escaped)
			{
				_escaped = false;
			}
			AutoReleaseWindow();
		}
		//关闭
		internal override void Shutdown()
		{
			CloseAllWindow();
			_commonRoot.Remove();
			ReleaseAllWindow(ReleaseType.NEVER);
			if (_releaseRoot != null)
			{
				GameObject.DestroyImmediate(_releaseRoot.gameObject);
				_releaseRoot = null;
			}
		}

		//根据一个窗口打开一个新窗口
		public Window OpenWindow(WNode openBy, string windowName, object[] param)
		{
			if (_switchingNode != null)
			{
				Log.Error($"request open window {windowName} during switch one other window {_switchingNode.NodeName}");
				return null;
			}
			if (openBy == null)
				return OpenWindow(windowName, param);

			// 打开一个与自己同名的界面
			if (openBy.NodeName == windowName)
			{
				Log.Error($"request open a same name child window {windowName}");
				return null;
			}

			if (!openBy.TryGetNode(windowName, out WNode openedWindow))
			{
				if (!_windowModeDic.TryGetValue(windowName, out WindowMode mode))
				{
					mode = GetModeFunc.Invoke(windowName);
					_windowModeDic.Add(windowName, mode);
				}
				//创建窗口
				openedWindow = CreateWindow(windowName, mode);
				openedWindow.rootNode = openBy.rootNode;
				openedWindow.transform.SetParent(openBy.rootNode.transform);
				openedWindow.transform.localPosition = Vector3.zero;
				openedWindow.transform.localRotation = Quaternion.identity;
				openedWindow.transform.localScale = Vector3.one;

			}
			//切换窗口
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
		public Window OpenWindow(string windowName, object[] param)
		{
			if (_switchingNode != null)
			{
				Log.Error($"request open window {windowName} during switch one other window {_switchingNode.NodeName}");
				return null;
			}

			Log.Debug($"open window {windowName}");

			if (!_windowModeDic.TryGetValue(windowName, out WindowMode mode))
			{
				mode = GetModeFunc.Invoke(windowName);
				_windowModeDic.Add(windowName, mode);
			}
			//找root节点，如果没有就新建一个
			WRootNode root = GetAndCreateRoot(mode.rootName);

			WNode window = null;
			if (!root.TryGetNode(windowName, out window))
			{
				//创建窗口
				window = CreateWindow(windowName, mode);
				window.rootNode = root;
				window.transform.SetParent(root.transform);
				window.transform.localPosition = Vector3.zero;
				window.transform.localRotation = Quaternion.identity;
				window.transform.localScale = Vector3.one;
				//归一
				window.transform.anchorMin = Vector3.zero;
				window.transform.anchorMax = Vector3.one;
				window.transform.sizeDelta = Vector3.zero;
			}
			//切换窗口
			SwitchWindow(window, root, param);

			return window as Window;
		}
		//打开根节点
		public WRootNode OpenRoot(string rootName, object[] param)
		{
			if (_switchingNode != null)
			{
				Log.Error($"request open window {rootName} during switch one other window {_switchingNode.NodeName}");
				return null;
			}

			if (string.IsNullOrEmpty(rootName)) return null;

            //找root节点，如果没有就新建一个
            WRootNode root = GetAndCreateRoot(rootName);
			//切换窗口
			SwitchWindow(root, null, param);

			return root;
		}
		//切换窗口
		private void SwitchWindow(WNode window, WNode parentNode, object[] param)
		{
			_switchingNode = window;
			//设置遮罩
			ShowMaskBox((int)MaskType.WINDOW_SWITCH, true);
			//准备切换
			window.Switch((canOpen) => SwitchEnd(window, parentNode, canOpen, param));
		}
		//窗口切换结束
		private void SwitchEnd(WNode window, WNode parentNode, bool canOpen, object[] param)
		{
			//关闭遮罩
			ShowMaskBox((int)MaskType.WINDOW_SWITCH, false);
			if (_switchingNode == null)
				return;
			if (_switchingNode != window)
				return;
            //切换中的窗口设为空
            _switchingNode = null;
			// 打开失败
			if (!canOpen)
			{
				//加入释放列表
				AddToReleaseQueue(window);
				return;
			}
			//是否需要屏幕遮罩
			bool switchScreen = false;
			var root = window.rootNode;

            //不是通用节点
            if (root != CommonRoot)
			{
                //如果是新创建的一个root
                if (root.stackIndex < 0)
				{
					switchScreen = true;
                    //把栈顶的节点暂停
                    if (_rootStack.Count > 0)
						_rootStack.Peek().Pause();
					root.stackIndex = _rootStack.Count;
					_rootStack.Push(root);
                    //打开的window就是root就把参数传进去，当前window不是root就只需要把root打开
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
                //如果root不是新创建的，是之前的
                else
                {
                    //判断当前window的root在不在栈顶
                    bool isTop = _rootStack.Count == window.rootNode.stackIndex + 1;
                    //如果不在按照顺序把后面的root都关掉
                    if (!isTop)
					{
						switchScreen = true;
						while (_rootStack.Peek() != root)
						{
							var top = _rootStack.Pop();
							top.Close();
						}
					}
                    //如果当前的window就是root
                    if (window == root)
					{
						root.Reset(param);
					}
					root.Resume();
				}
			}

            //如果window是窗口,不是root节点
            if (window is Window)
			{
                //如果root里存在这个窗口
                if (root.Contains(window))
				{
                    //如果窗口的父级是root节点，并且窗口是关键节点或者是全屏的，则关闭root节点下除了window以外的所有节点
                    // 保持队列的顺序不变,不能循环打开
                    if (window.parentNode == root && (window.IsFullScreen || window.IsMainNode))
					{
						for (int i = root.ChildNode.Count - 1; i >= 0; i--)
						{
							//把当前窗口后面的窗口全部关闭
							if (root.ChildNode[i] == window)
								break;
							root.ChildNode[i].Close();
						}
					}
                    //设置父级，重置窗口
                    window.parentNode = parentNode;
					window.Reset(param);
					window.Resume();
				}
                //如果不存在这个窗口
                else
                {
                    //设置遮罩
                    int mask = ((Window)window).WindowMode.showScreenMask;
					if (mask == 2)
						switchScreen = true;
					else if (mask == 1 && window.newCreate)
						switchScreen = true;

                    //设置父级，打开窗口
                    window.parentNode = parentNode;
					window.Open(param);
					window.Resume();
				}
			}
			if (switchScreen)
			{
				ShowScreenMask();
			}
		}

        //找root节点，如果没有就新建一个
        private WRootNode GetAndCreateRoot(string rootName)
		{
			if (string.IsNullOrEmpty(rootName))
			{
				Debug.Assert(_rootStack.Count > 0);
				//返回栈顶
				return _rootStack.Peek();
			}
			//如果是通用节点
			if (_commonRoot != null && rootName == _commonRoot.NodeName)
				return _commonRoot;

			//找根节点
			foreach (var item in _rootStack)
			{
				if (item.NodeName.Equals(rootName))
				{
					return item;
				}
			}
			//从释放列表里找回来根节点
			WRootNode root = null;
			for (int i = 0; i < _waitReleaseWindow.Count; i++)
			{
				if (_waitReleaseWindow[i].NodeName.Equals(rootName))
				{
					root = _waitReleaseWindow[i] as WRootNode;
					_waitReleaseWindow.RemoveAt(i);
					break;
				}
			}
			//创建根节点
			if (root == null)
			{
				root = new WRootNode(rootName);
				root.gameObject = new GameObject(rootName);
				root.transform = root.gameObject.AddComponent<RectTransform>();
				root.Start();
			}

			root.transform.SetParent(WindowRoot);
			root.transform.localPosition = Vector3.zero;
			root.transform.localScale = Vector3.one;
			root.transform.localRotation = Quaternion.identity;
            //归一
            root.transform.anchorMin = Vector3.zero;
            root.transform.anchorMax = Vector3.one;
            root.transform.sizeDelta = Vector3.zero;

            //新建的根节点 索引一定要设置-1
            root.stackIndex = -1;
			return root;
		}

		//创建窗口
		private Window CreateWindow(string windowName, WindowMode mode)
		{
            //从释放列表里找回来窗口
            Window window = null;
			for (int i = 0; i < _waitReleaseWindow.Count; i++)
			{
				if (_waitReleaseWindow[i].NodeName.Equals(windowName))
				{
					window = _waitReleaseWindow[i] as Window;
					_waitReleaseWindow.RemoveAt(i);
					break;
				}
			}
			//创建窗口
			if (window == null)
			{
				window = new Window(windowName, mode);
				window.CreateWindowView();
				SortDepthFunc?.Invoke(window.gameObject, mode.depth);
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
		public object CloseWindow(string windowClose)
		{
			if (string.IsNullOrEmpty(windowClose)) return null;
			//从通用节点尝试关闭窗口
			if (_commonRoot.TryCloseChild(windowClose, out object val))
			{
				return val;
			}

			if (_rootStack.Count == 0) return null;

			Log.Debug($"close window {windowClose}");

			WRootNode root = _rootStack.Peek();

			// 关闭子窗口
			if (root.TryCloseChild(windowClose, out val))
			{
				return val;
			}
			return null;
		}

        //栈顶节点关闭互斥子节点
        public void CloseWindow(int windowFlag)
		{
			if (_rootStack.Count == 0) return;
			if (0 == windowFlag) return;

			WRootNode root = _rootStack.Peek();
			//栈顶节点关闭互斥子节点
			root.CloseChild(windowFlag);
		}

		//关闭全部窗口
		public void CloseAllWindow()
		{
			//如果有切换中的节点，则加入释放列表
			if (_switchingNode != null)
			{
				AddToReleaseQueue(_switchingNode);
				_switchingNode = null;
			}
			//清理通用根节点的子节点
			CommonRoot.CloseAllChild();
			//清理栈
			while (_rootStack.Count > 0)
			{
				_rootStack.Pop().Close();
			}
		}
        //返回键请求关闭窗口处理
        public void EscapeTopWindow()
		{
			EscapeType escape = EscapeType.AUTO_CLOSE;
			//处理通用节点
			if (_commonRoot.Escape(ref escape))
				return;

			if (_rootStack.Count == 0) return;
			//处理栈顶
			var top = _rootStack.Peek();
			top.Escape(ref escape);
		}


		/// <summary>
		/// 关闭root时从栈内移除
		/// </summary>
		/// <param name="root"></param>
		public void RemoveRoot(WRootNode root)
		{
			if (_rootStack.Count == 0) return;

            //如果当前root是栈顶，则移出去
            if (root == _rootStack.Peek())
			{
				_rootStack.Pop();

				if (_rootStack.Count > 0)
					_rootStack.Peek().Resume();
			}
            //如果不是栈顶
            else
            {
				var index = 0;
				foreach (var node in _rootStack)
				{
					if (node == root)
						break;
					index++;
				}

                //如果相等说明这个root不在栈里管理，不需要重新计算栈，直接return
                if (index == _rootStack.Count)
					return;
				Stack<WRootNode> tempStack = new Stack<WRootNode>();

				while (index >= 0)
				{
                    //这里会提前pop出来，index==0之后不会把这个塞进去（index==0这个是要移除的）
                    var top = _rootStack.Pop();
					if (index-- == 0)
					{

					}
					else
						tempStack.Push(top);
				}

                //按照原来栈顺序重新塞回来，并且重新计算stackIndex
                while (tempStack.Count > 0)
				{
					var temp = tempStack.Pop();
					temp.stackIndex = _rootStack.Count;
					_rootStack.Push(temp);
				}

			}
			if (_rootStack.Count == 0)
			{
				OnClosedLastRootFunc?.Invoke();
			}
		}
		//释放节点
		private RectTransform _releaseRoot;
		//增加到释放队列
		public void AddToReleaseQueue(WNode node)
		{
			if (node != null)
			{
				//如果是自动的，设置时间
				if (node.releaseType == ReleaseType.AUTO)
				{
					node.releaseTimer = autoCacheTime;
				}
				else
				{
					node.releaseTimer = 0;
				}
				//创建释放节点
				if (_releaseRoot == null)
				{
					_releaseRoot = new GameObject("WaitForRelease").AddComponent<RectTransform>();
					_releaseRoot.SetParent(WindowRoot);
					_releaseRoot.localScale = Vector3.one;
					_releaseRoot.localPosition = new Vector3(30000, 0, 0);
					//归一
                    _releaseRoot.anchorMin = Vector3.zero;
                    _releaseRoot.anchorMax = Vector3.one;
                    _releaseRoot.sizeDelta = Vector3.zero;
                }

				if (node.transform != null)
				{
					node.transform.parent = _releaseRoot;
				}
				//增加进去
				_waitReleaseWindow.Add(node);
			}
		}

		//遍历释放列表
		private void AutoReleaseWindow()
		{
			for (int i = _waitReleaseWindow.Count - 1; i >= 0; i--)
			{
				//如果可以移除
				if (_waitReleaseWindow[i].AutoRemove())
				{
					_waitReleaseWindow.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// 释放全部window资源
		/// </summary>
		/// <param name="type">筛选释放window的级别，释放小于这个级别的所有窗口</param>
		public void ReleaseAllWindow(ReleaseType level = ReleaseType.AUTO)
		{
			for (int i = _waitReleaseWindow.Count - 1; i >= 0; i--)
			{
				if (_waitReleaseWindow[i].releaseType <= level)
				{
					_waitReleaseWindow[i].Remove();
					_waitReleaseWindow.RemoveAt(i);
				}
			}
		}
		//增加节点关闭回调
		public void AddCloseCallback(string windowName, Action<object> callback)
		{
			if (_windowCloseCallback.TryGetValue(windowName, out Action<object> action))
			{
				action -= callback;
				action += callback;
			}
			else
			{
				_windowCloseCallback.Add(windowName, callback);
			}
		}
		//移除节点关闭回调
		public void RemoveCloseCallback(string windowName, Action<object> callback)
		{
			if (_windowCloseCallback.TryGetValue(windowName, out Action<object> action))
			{
				action -= callback;
			}
		}
		//触发节点关闭回调
		public void OnWindowClose(string windowName, object backValue)
		{
			if (_windowCloseCallback.TryGetValue(windowName, out Action<object> action))
			{
				action?.Invoke(backValue);
			}
		}
		//获取窗口，根据窗口名称
		public Window GetWindow(string windowName)
		{
			if (_rootStack.Count == 0) return null;
			//在栈顶根节点找子节点
			var top = _rootStack.Peek();
			if (top.TryGetNode(windowName, out var node))
			{
				return node as Window;
			}

			return null;
		}
		//获取根节点，根据根节点名称
		public WRootNode GetRoot(string rootName)
		{
			if (_rootStack.Count == 0) return null;

			foreach (var root in _rootStack)
			{
				if (root.NodeName == rootName)
					return root;
			}

			return null;
		}
		//获取栈顶的根节点
		public WRootNode GetTopRoot()
		{
			if (_rootStack.Count == 0) return null;

			return _rootStack.Peek();
		}
		//获取栈顶的最新窗口
		public Window GetTopWindow()
		{
			if (_rootStack.Count == 0) return null;

			var peekWindow = _rootStack.Peek();
			if (peekWindow != null)
				return peekWindow.GetTopWindow() as Window;
			return null;
		}

		/// <summary>
		/// 显示一个碰撞框，不能再次点击
		/// </summary>
		/// <param name="maskType"></param>
		public void ShowMaskBox(int maskType, bool enable)
		{
			ShowMaskBoxFunc?.Invoke(maskType, enable);
		}

		/// <summary>
		/// 屏幕遮黑淡入
		/// 替换以前的截屏操作，这个更快，不需要等待一帧
		/// </summary>
		public void ShowScreenMask()
		{
			ShowScreenMaskFunc?.Invoke();
		}


	}
}
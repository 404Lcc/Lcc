using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
	internal class WindowManager : Module, IWindowService
	{
		/// <summary>
		/// 当前活动窗口的栈
		/// 栈里的每个窗口实际是一个全屏窗口和从属于这个全屏窗口的子窗口
		/// 每个窗口的作用域是自己和从属于自己的子窗口，不能跨域修改其它窗口
		/// </summary>
		private Stack<DomainNode> _rootStack = new Stack<DomainNode>();

		/// <summary>
		/// 当前活动的通用窗口
		/// 这些特殊窗口不受栈的限制，可以用任意方式唤醒和关闭
		/// </summary>
		private DomainNode _commonRoot;

		public DomainNode CommonRoot => _commonRoot;

		/// <summary>
		/// 等待释放的窗口
		/// </summary>
		private List<UINode> _waitReleaseWindow = new List<UINode>();

		/// <summary>
		/// 被关闭界面会自动缓存多少帧然后释放
		/// 30s
		/// </summary>
		public int autoCacheTime = 900;

		/// <summary>
		/// 窗口关闭回调
		/// </summary>
		private Dictionary<string, Action<object>> _windowCloseCallback = new Dictionary<string, Action<object>>();

		//当前切换中的窗口
		private UINode _switchingNode;

		private AssetLoader _assetLoader = new AssetLoader();

		//需要更新的节点列表
		private List<UINode> _updateNodes = new List<UINode>();

		//释放节点
		private RectTransform _releaseRoot;

		/// <summary>
		/// 获取窗口的父节点
		/// </summary>
		public Transform WindowRoot { get; set; }

		/// <summary>
		/// ui相机
		/// </summary>
		public Camera UICamera { get; set; }

		private Dictionary<UILayerID, UILayer> _uiLayerDict = new Dictionary<UILayerID, UILayer>();

		private Dictionary<string, Type> _uiLogics = new Dictionary<string, Type>();
		/// <summary>
		/// 异步加载GameObject
		/// </summary>
		public Action<AssetLoader, string, Action<GameObject>> LoadAsyncGameObject { get; set; }

		public UIRoot Root;

		//初始化获取logic类
		public void InitializeForAssembly(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (Type t in types)
			{
				if (typeof(IUILogic).IsAssignableFrom(t))
				{
					_uiLogics[t.Name] = t;
				}
			}
		}
		

		//根据logic名称，创建logic
		public IUILogic CreateLogic(string logicName, UINode node)
		{
			Debug.Assert(!string.IsNullOrEmpty(logicName));

			IUILogic logic = null;
			if (_uiLogics.TryGetValue(logicName, out Type monoType))
			{
				logic = Activator.CreateInstance(monoType) as IUILogic;
				logic.Node = node;
			}

			return logic;
		}


		//初始化通用节点
		public void Init()
		{
			Root = new UIRoot(WindowRoot.gameObject);
			_commonRoot = GetAndCreateRoot("UIRootCommon");
			_commonRoot.StackIndex = 0;
			_commonRoot.Show(null);

			for (UILayerID layerId = UILayerID.HUD; layerId <= UILayerID.Debug; layerId++)
			{
				var layer = new UILayer(Root, layerId);
				layer.Create(WindowRoot);
				_uiLayerDict[layerId] = layer;
			}
		}

		public UILayer GetUILayer(UILayerID layerID)
		{
			if (_uiLayerDict.TryGetValue(layerID, out var layer))
			{
				return layer;
			}

			return null;
		}


		internal override void Update(float elapseSeconds, float realElapseSeconds)
		{
			if (_commonRoot != null)
			{
				_updateNodes.Clear();
				_commonRoot.GetAllChildNode(ref _updateNodes);
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
				peekWindow.GetAllChildNode(ref _updateNodes);
				foreach (var node in _updateNodes)
				{
					node.Update();
				}
			}
			Debug.LogError(peekWindow.NodeName);
		}


		internal override void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				EscapeTopWindow();
			}

			AutoReleaseWindow();
		}

		//关闭
		internal override void Shutdown()
		{
			CloseAllWindow();
			_commonRoot.Destroy();
			Root.Detach(_commonRoot);
			ReleaseAllWindow(ReleaseType.NEVER);
			if (_releaseRoot != null)
			{
				GameObject.DestroyImmediate(_releaseRoot.gameObject);
				_releaseRoot = null;
			}
		}


		//切换窗口
		private void SwitchWindow(UINode window, object[] param)
		{
			//准备切换
			window.Switch((canOpen) => SwitchEnd(window, canOpen, param));
		}

		//窗口切换结束
		private void SwitchEnd(UINode window, bool canOpen, object[] param)
		{
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

			var root = window.DomainNode;

			//不是通用节点
			if (root != CommonRoot)
			{
				//如果是新创建的一个root
				if (root.StackIndex < 0)
				{
					//把栈顶的节点暂停
					if (_rootStack.Count > 0)
					{
						_rootStack.Peek().Covered(true);
					}

					root.StackIndex = _rootStack.Count;
					_rootStack.Push(root);

					root.Covered(false);

					//打开的window就是root就把参数传进去，当前window不是root就只需要把root打开
					if (window == root)
					{
						root.Show(param);
					}
					else
					{
						root.Show(null);
					}
				}
				//如果root不是新创建的，是之前的
				else
				{
					//判断当前window的root在不在栈顶
					bool isTop = _rootStack.Count == window.DomainNode.StackIndex + 1;
					//如果不在按照顺序把后面的root都关掉
					if (!isTop)
					{
						while (_rootStack.Peek() != root)
						{
							var top = _rootStack.Pop();
							top.Hide();
						}
					}

					root.Covered(false);

					//如果当前的window就是root
					if (window == root)
					{
						root.ReShow(param);
					}


				}
			}

			//如果window是窗口,不是root节点
			if (window is ElementNode w)
			{
				//如果root里存在这个窗口
				if (root.ContainsNode(window))
				{
					//如果窗口的父级是root节点，并且窗口是关键节点或者是全屏的，则关闭root节点下除了window以外的所有节点
					// 保持队列的顺序不变,不能循环打开
					if (w.IsFullScreen)
					{
						for (int i = root.NodeList.Count - 1; i >= 0; i--)
						{
							//把当前窗口后面的窗口全部关闭
							if (root.NodeList[i] == window)
								break;
							root.NodeList[i].Hide();
						}
					}

					//设置父级，重置窗口
					window.Covered(false);
					window.ReShow(param);
				}
				//如果不存在这个窗口
				else
				{
					//设置父级，打开窗口
					window.Covered(false);
					window.Show(param);
				}
			}
		}

		//找root节点，如果没有就新建一个
		private DomainNode GetAndCreateRoot(string rootName)
		{
			if (string.IsNullOrEmpty(rootName))
			{
				Debug.Assert(_rootStack.Count > 0);
				//返回栈顶
				return _rootStack.Peek();
			}

			//如果是通用节点
			if (_commonRoot != null && rootName == _commonRoot.NodeName)
			{
				return _commonRoot;
			}

			//找根节点
			foreach (var item in _rootStack)
			{
				if (item.NodeName.Equals(rootName))
				{
					return item;
				}
			}

			//从释放列表里找回来根节点
			DomainNode root = null;
			for (int i = 0; i < _waitReleaseWindow.Count; i++)
			{
				if (_waitReleaseWindow[i].NodeName.Equals(rootName))
				{
					root = _waitReleaseWindow[i] as DomainNode;
					_waitReleaseWindow.RemoveAt(i);
					break;
				}
			}

			//创建根节点
			if (root == null)
			{
				root = new DomainNode(rootName);
				root.Init();
				Root.Attach(rootName, root);
				root.Create();
			}

			//新建的根节点 索引一定要设置-1
			root.StackIndex = -1;
			return root;
		}

		
		private ElementNode GetOrCreateWindow(string windowName, out bool isNewCreate)
		{
			isNewCreate = false;
    
			var windowFromWaitList = TryGetWindowFromWaitList(windowName);
			if (windowFromWaitList != null)
			{
				return windowFromWaitList;
			}

			var newWindow = new ElementNode(windowName);
			newWindow.Logic = CreateLogic(windowName, newWindow);
			newWindow.Init();
			Root.Attach(windowName, newWindow);
			isNewCreate = true;
			return newWindow;
		}
		
		private ElementNode TryGetWindowFromWaitList(string windowName)
		{
			//从释放列表里找回来窗口
			for (int i = 0; i < _waitReleaseWindow.Count; i++)
			{
				if (_waitReleaseWindow[i].NodeName.Equals(windowName))
				{
					var window = _waitReleaseWindow[i] as ElementNode;
					_waitReleaseWindow.RemoveAt(i);
					return window;
				}
			}
			
			return null;
		}



		//遍历释放列表
		private void AutoReleaseWindow()
		{
			for (int i = _waitReleaseWindow.Count - 1; i >= 0; i--)
			{
				//如果可以移除
				if (_waitReleaseWindow[i].AutoRemove())
				{
					_waitReleaseWindow[i].Destroy();
					Root.Detach(_waitReleaseWindow[i]);
					_waitReleaseWindow.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// 打开一个界面
		/// 这里只是创建，并不会改变当前栈结构
		/// 确认界面可打开后才会继续
		/// </summary>
		/// <param name="windowName"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public void OpenWindow(string windowName, object[] param)
		{
			if (_switchingNode != null)
			{
				Log.Error($"request open window {windowName} during switch one other window {_switchingNode.NodeName}");
				return;
			}

			Log.Debug($"open window {windowName}");


			//找root节点，如果没有就新建一个
			var root = GetAndCreateRoot(string.Empty);

			if (!root.TryGetChildNode(windowName, out var window))
			{
				window = GetOrCreateWindow(windowName, out var isNewCreate);
				_switchingNode = window;
				window.DomainNode = root;
				if (isNewCreate)
				{
					window.CreateElement(_assetLoader, (window) =>
					{
						window.Create();
						//切换窗口
						SwitchWindow(window, param);
					});
				}
				else
				{
					//切换窗口
					SwitchWindow(window, param);
				}
			}
			else
			{
				_switchingNode = window;
				window.DomainNode = root;
				//切换窗口
				SwitchWindow(window, param);
			}
		}
		
		/// <summary>
		/// 打开一个界面
		/// 这里只是创建，并不会改变当前栈结构
		/// 确认界面可打开后才会继续
		/// </summary>
		/// <param name="windowName"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public void OpenWindow(string windowName, string rootName, object[] param)
		{
			if (_switchingNode != null)
			{
				Log.Error($"request open window {windowName} during switch one other window {_switchingNode.NodeName}");
				return;
			}

			Log.Debug($"open window {windowName}");


			//找root节点，如果没有就新建一个
			var root = GetAndCreateRoot(rootName);

			if (!root.TryGetChildNode(windowName, out var window))
			{
				window = GetOrCreateWindow(windowName, out var isNewCreate);
				_switchingNode = window;
				//创建窗口
				window.DomainNode = root;
				if (isNewCreate)
				{
					window.CreateElement(_assetLoader, (window) =>
					{
						window.Create();
						//切换窗口
						SwitchWindow(window, param);
					});
				}
			}
			else
			{
				_switchingNode = window;
				window.DomainNode = root;
				//切换窗口
				SwitchWindow(window, param);
			}
		}
		
		//打开根节点
		public DomainNode OpenRoot(string rootName, object[] param)
		{
			if (_switchingNode != null)
			{
				Log.Error($"request open window {rootName} during switch one other window {_switchingNode.NodeName}");
				return null;
			}

			if (string.IsNullOrEmpty(rootName))
				return null;

			//找root节点，如果没有就新建一个
			DomainNode root = GetAndCreateRoot(rootName);
			root.DomainNode = root;
			_switchingNode = root;
			//切换窗口
			SwitchWindow(root, param);

			return root;
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
			if (string.IsNullOrEmpty(windowClose))
				return null;
			//从通用节点尝试关闭窗口
			if (_commonRoot.TryHideChildNode(windowClose, out object val))
			{
				return val;
			}

			if (_rootStack.Count == 0)
				return null;

			Log.Debug($"close window {windowClose}");

			DomainNode root = _rootStack.Peek();

			// 关闭子窗口
			if (root.TryHideChildNode(windowClose, out val))
			{
				return val;
			}

			return null;
		}

		//关闭全部窗口
		public void CloseAllWindow()
		{
			//todo 判断如果有资源加载完成才能加进去
			//如果有切换中的节点，则加入释放列表
			if (_switchingNode != null)
			{
				AddToReleaseQueue(_switchingNode);
				_switchingNode = null;
			}

			//清理通用根节点的子节点
			CommonRoot.Hide();
			//清理栈
			while (_rootStack.Count > 0)
			{
				_rootStack.Pop().Hide();
			}
		}

		//返回键请求关闭窗口处理
		public void EscapeTopWindow()
		{
			EscapeType escape = EscapeType.Hide;
			//处理通用节点
			if (_commonRoot.Escape(ref escape))
				return;

			if (_rootStack.Count == 0)
				return;
			//处理栈顶
			var top = _rootStack.Peek();
			top.Escape(ref escape);
		}

		/// <summary>
		/// 关闭root时从栈内移除
		/// </summary>
		/// <param name="root"></param>
		public void RemoveRoot(DomainNode root)
		{
			if (_rootStack.Count == 0)
				return;

			//如果当前root是栈顶，则移出去
			if (root == _rootStack.Peek())
			{
				_rootStack.Pop();

				if (_rootStack.Count > 0)
				{
					_rootStack.Peek().Covered(false);
				}
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
				Stack<DomainNode> tempStack = new Stack<DomainNode>();

				while (index >= 0)
				{
					//这里会提前pop出来，index==0之后不会把这个塞进去（index==0这个是要移除的）
					var top = _rootStack.Pop();
					if (index-- == 0)
					{

					}
					else
					{
						tempStack.Push(top);
					}
				}

				//按照原来栈顺序重新塞回来，并且重新计算stackIndex
				while (tempStack.Count > 0)
				{
					var temp = tempStack.Pop();
					temp.StackIndex = _rootStack.Count;
					_rootStack.Push(temp);
				}

			}
		}

		//增加到释放队列
		public void AddToReleaseQueue(UINode node)
		{
			if (node != null)
			{
				//如果是自动的，设置时间
				if (node.ReleaseType == ReleaseType.AUTO)
				{
					node.ReleaseTimer = autoCacheTime;
				}
				else
				{
					node.ReleaseTimer = 0;
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

				if (node is ElementNode window)
				{
					if (window.RectTransform != null)
					{
						window.RectTransform.parent = _releaseRoot;
					}
				}

				//增加进去
				_waitReleaseWindow.Add(node);
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
				if (_waitReleaseWindow[i].ReleaseType <= level)
				{
					_waitReleaseWindow[i].Destroy();
					Root.Detach(_waitReleaseWindow[i]);
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
		public ElementNode GetWindow(string windowName)
		{
			if (_rootStack.Count == 0) return null;
			//在栈顶根节点找子节点
			var top = _rootStack.Peek();
			if (top.TryGetChildNode(windowName, out var node))
			{
				return node as ElementNode;
			}

			return null;
		}

		//获取根节点，根据根节点名称
		public DomainNode GetRoot(string rootName)
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
		public DomainNode GetTopRoot()
		{
			if (_rootStack.Count == 0)
				return null;

			return _rootStack.Peek();
		}

		//获取栈顶的最新窗口
		public ElementNode GetTopWindow()
		{
			if (_rootStack.Count == 0)
				return null;

			var peekWindow = _rootStack.Peek();
			if (peekWindow != null)
			{
				return peekWindow.GetTopNode() as ElementNode;
			}

			return null;
		}
	}
}
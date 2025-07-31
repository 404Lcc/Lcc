using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
	public abstract class WNode
	{
		/// <summary>
		/// 用于保存关闭时的处理
		/// </summary>
		public class TurnNode
		{
			public string nodeName;
			public NodeType nodeType;
			public object[] nodeParam;
		}

		/// <summary>
		/// 窗口的状态
		/// </summary>
		protected NodePhase _nodePhase;
		//是否激活
		public bool Active => _nodePhase == NodePhase.ACTIVE;

		protected string _nodeName;
		public string NodeName => _nodeName;
		//根节点
		public WRootNode rootNode;
		//父节点
		public WNode parentNode;

		/// <summary>
		/// 全屏界面可以包含许多子界面
		/// </summary>
		protected List<WNode> _childNode;
		public List<WNode> ChildNode => _childNode;

		/// <summary>
		/// 关闭后会返回的界面
		/// </summary>
		public TurnNode returnNode;

		public bool newCreate = true;

		/// <summary>
		/// 用于处理两个窗口彼此互斥的情况
		/// 从属于同一窗口的子窗口flag的 & 运算结果不能大于0
		/// </summary>
		public int RejectFlag { get; protected set; }
		//节点类型
		public int NodeFlag { get; protected set; }
		//是否全屏窗口
		public bool IsFullScreen => (NodeFlag & (int)LccHotfix.NodeFlag.FULL_SCREEN) > 0;
		//是否主窗口
		public bool IsMainNode => (NodeFlag & (int)LccHotfix.NodeFlag.MAIN_NODE) > 0;
		//是否顶层窗口
		public bool IsTopNode => (NodeFlag & (int)LccHotfix.NodeFlag.TOP_NODE) > 0;
		//回退类型
		public EscapeType escapeType;
		//释放窗口类型
		public ReleaseType releaseType = ReleaseType.AUTO;

		public int releaseTimer;

		protected GameObject _gameObject;
		public GameObject gameObject
		{
			get => _gameObject;
			set => _gameObject = value;
		}

		protected RectTransform _transform;
		public RectTransform transform
		{
			get => _transform;
			set => _transform = value;
		}
		//窗口逻辑
		protected IUILogic _logic;
		public IUILogic Logic
		{
			get => _logic;
			set => _logic = value;
		}

		protected string _logicName;
		public string LogicName
		{
			get => _logicName;
			set => _logicName = value;
		}

        //判断是否包含节点（包含自身）
        public bool Contains(WNode node)
		{
			if (node == this) return true;
			if (_childNode == null) return false;
			foreach (WNode childNode in _childNode)
			{
				if (childNode.Contains(node))
					return true;
			}
			return false;
		}

        //获取节点（包含自身）
        public bool TryGetNode(string windowName, out WNode node)
		{
			node = null;
			if (_nodeName.Equals(windowName))
			{
				node = this;
				return true;
			}
			if (_childNode == null || _childNode.Count == 0) return false;
			foreach (var childNode in _childNode)
			{
				if (childNode.TryGetNode(windowName, out node))
				{
					return true;
				}
			}
			return false;
		}

        //从下向上，判断这个节点是否存在，存在则返回节点的父节点
        public bool TryGetNodeForward(string windowName, out WNode node)
		{
			node = null;

			if (parentNode == null) return false;
			if (parentNode.NodeName == windowName)
			{
				node = parentNode;
				return true;
			}

			if (parentNode._childNode != null && parentNode._childNode.Count > 0)
			{
				foreach (var child in parentNode._childNode)
				{
					if (child.NodeName == windowName)
					{
						node = parentNode;
						return true;
					}
				}
			}

			return parentNode.TryGetNodeForward(windowName, out node);
		}

        //获取当前节点下最新的节点
        public WNode GetTopWindow()
		{
			if (_childNode == null || _childNode.Count == 0) return this;
			return _childNode[_childNode.Count - 1].GetTopWindow();
		}

		public void Start()
		{
			_nodePhase = NodePhase.DEACTIVE;
			DoStart();
		}

		public void Update()
		{
			if (_nodePhase == NodePhase.ACTIVE)
			{
				DoUpdate();
			}
		}
		public void Open(object[] param)
		{

			if (_nodePhase == NodePhase.DEACTIVE)
			{
				if (parentNode != null && parentNode._nodePhase < NodePhase.OPENED) return;
				Log.Debug($"ui open window {NodeName}");
				newCreate = false;
				//把自己节点状态设置为激活
				_nodePhase = NodePhase.OPENED;
                //如果有父节点则把自己加进父级的子节点
                if (parentNode != null)
					parentNode.ChildOpened(this);
				DoOpen(param);

			}
		}

		public void Reset(object[] param)
		{
			if (_nodePhase >= NodePhase.OPENED)
			{
				DoReset(param);
			}
		}

		//父级调用打开孩子
		public void ChildOpened(WNode child)
		{
			if (_childNode == null)
				_childNode = new List<WNode>();
			// 检查节点标记
			if (_childNode != null && _childNode.Count > 0)
			{
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
                    //如果是互斥的界面就关掉
                    if ((_childNode[i].RejectFlag & child.RejectFlag) > 0)
					{
						_childNode[i].Close();
					}
				}
			}
			//加进子节点
			_childNode.Add(child);

			if (_childNode != null && _childNode.Count > 1)
			{
				//找到全屏窗口索引，这里遍历包含刚才加入的节点
				int fullIndex = _childNode.Count;
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
					fullIndex = i;
					if (_childNode[i].IsFullScreen)
					{
						break;
					}
				}
                //如果没找到全屏窗口fullIndex就是_childNode.Count。-2是排除刚才加入的节点
                //遍历子节点，排除刚才加入的节点（排除顶层节点，假如_childNode.count是10个节点，fullIndex是5，则暂停5以前的节点，恢复5到8节点。）
                //刚才加入的节点在后面调用Resume
                for (int i = _childNode.Count - 2; i >= 0; i--)
				{
                    //小于全屏索引的节点并且不是顶层窗口的节点全部暂停，否则调用恢复窗口
					//用小于是因为全屏窗口不要暂停
					//主要节点也会暂停
                    if (i < fullIndex && !_childNode[i].IsTopNode)
						_childNode[i].Pause();
					else
						_childNode[i].Resume();
				}

			}

			DoChildOpened(child);
		}

		public void Resume()
		{
			//如果是暂停状态
			if (_nodePhase == NodePhase.OPENED)
			{
				if (parentNode != null && parentNode._nodePhase < NodePhase.ACTIVE) return;
				Log.Debug($"ui resume window {NodeName}");
				//设置成激活
				_nodePhase = NodePhase.ACTIVE;
				DoResume();
				if (_childNode != null && _childNode.Count > 0)
                {
					//找到全屏窗口索引
                    int fullIndex = _childNode.Count;
					for (int i = _childNode.Count - 1; i >= 0; i--)
					{
						fullIndex = i;
						if (_childNode[i].IsFullScreen)
						{
							break;
						}
					}

					//找到全屏窗口后面的节点，包含全屏窗口
					for (int i = fullIndex; i < _childNode.Count; i++)
					{
						//给子节点恢复
						_childNode[i].Resume();
					}
				}
			}
		}
		public void Pause()
		{
			//如果是激活状态
			if (_nodePhase == NodePhase.ACTIVE)
			{
				Log.Debug($"ui pause window {NodeName}");
				DoPause();
				//设置成暂停
				_nodePhase = NodePhase.OPENED;
				//给子节点全部暂停
				if (_childNode != null && _childNode.Count > 0)
				{
					for (int i = _childNode.Count - 1; i >= 0; i--)
					{
						_childNode[i].Pause();
					}
				}
			}
		}

		public object Close()
		{
			//如果激活状态，先暂停
			if (_nodePhase == NodePhase.ACTIVE)
			{
				Pause();
			}
			//如果是暂停状态
			if (_nodePhase == NodePhase.OPENED)
			{
				Log.Debug($"ui close window {NodeName}");
				//如果有父级
				// 由下向上
				if (parentNode != null)
				{
					//移除从父级移除当前节点
					parentNode.ChildClosed(this);
				}
				//如果是当前节点是根节点，则移除根节点
				else if (this is WRootNode)
				{
					Main.WindowService.RemoveRoot(this as WRootNode);
				}
				//移除当前节点的子节点
				// 由上向下
				while (_childNode != null && _childNode.Count > 0)
				{
					var child = _childNode[_childNode.Count - 1];
					_childNode.RemoveAt(_childNode.Count - 1);
					child.parentNode = null;
					child.Close();
				}
				_childNode = null;
				returnNode = null;
				//设置关闭状态
				_nodePhase = NodePhase.DEACTIVE;
				var returnValue = DoClose();

				return returnValue;
			}
			return null;
		}
		public void ChildClosed(WNode child)
		{
			if (_childNode == null)
				return;
			//移除节点
			_childNode.Remove(child);
			child.parentNode = null;
			child.rootNode = null;

			DoChildClosed(child);


		}
		/// <summary>
		/// 从内存中移除
		/// </summary>
		public void Remove()
		{
			DoRemove();
		}

		public void Switch(Action<bool> callback)
		{
			DoSwitch(callback);
		}

		/// <summary>
		/// 返回键请求关闭窗口处理
		/// </summary>
		/// <param name="escape"></param>
		/// <returns></returns>
		public bool Escape(ref EscapeType escape)
		{
			// 首先处理子节点的返回
			if (_childNode != null && _childNode.Count > 0)
			{
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
					if (_childNode[i].Escape(ref escape))
					{
						return true;
					}
				}
			}
			// 处理自己的返回
			return DoEscape(ref escape);
		}


		//处理子节点退出
		public bool ChildRequireEscape(WNode child)
		{
			//判断条件
			if (DoChildRequireEscape(child))
			{
				child.Close();
				return true;
			}
			return false;
		}

		//开始
		protected virtual void DoStart()
		{

		}

		//更新
		protected virtual void DoUpdate()
		{

		}

		//打开
		protected virtual void DoOpen(object[] param)
		{

		}

		//重置
		protected virtual void DoReset(object[] param)
		{

		}

		//恢复
		protected virtual void DoResume()
		{

		}

		//暂停
		protected virtual void DoPause()
		{

		}

		//关闭
		protected virtual object DoClose()
		{
			return null;
		}

		//移除（彻底关闭）
		protected virtual void DoRemove()
		{

		}

		//切换窗口
		protected virtual void DoSwitch(Action<bool> callback)
		{

		}

		//子节点打开
		protected virtual void DoChildOpened(WNode child)
		{

		}

		//子节点关闭
		protected virtual void DoChildClosed(WNode child)
		{

		}

		/// <summary>
		/// 子节点请求推出
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		protected virtual bool DoChildRequireEscape(WNode child)
		{
			return true;
		}

		//处理窗口返回
		protected virtual bool DoEscape(ref EscapeType escape)
		{
			escape = this.escapeType;
			if (escape == EscapeType.SKIP_OVER)
				return false;
			if (escape == EscapeType.AUTO_CLOSE && parentNode != null)
			{
				if (!parentNode.ChildRequireEscape(this))
				{
					escape = EscapeType.REFUSE_AND_BREAK;
					return false;
				}
			}
			return true;
		}

		//尝试关闭子窗口，根据名称
		public bool TryCloseChild(string windowClose, out object val)
		{
			val = null;
			//如果是自己是关闭
			if (windowClose == NodeName)
			{
				val = Close();
				return true;
			}
			else
			{
				//如果不是就找子节点
				if (_childNode == null || _childNode.Count == 0) return false;
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
					if (_childNode[i].TryCloseChild(windowClose, out val))
						return true;
				}
			}

			return false;
		}

		//关闭子窗口，根据互斥
		public void CloseChild(int flag)
		{
			if ((RejectFlag & flag) > 0)
			{
				Close();
			}
			if (_childNode == null || _childNode.Count == 0) return;
			for (int i = _childNode.Count - 1; i >= 0; i--)
			{
				_childNode[i].CloseChild(flag);
			}
		}

		//关闭所有子窗口
		public void CloseAllChild()
		{
			if (_childNode == null || _childNode.Count == 0) return;
			for (int i = _childNode.Count - 1; i >= 0; i--)
			{
				_childNode[i].Close();
			}
		}

		//更新移除
		public bool AutoRemove()
		{
			if (releaseType > ReleaseType.AUTO)
				return false;
			if (--releaseTimer <= 0)
			{
				Remove();
				return true;
			}
			return false;
		}


		/// <summary>
		/// 递归的获取自身及所有子节点
		/// </summary>
		/// <param name="childList"></param>
		public void GetAllChild(List<WNode> childList)
		{
			childList.Add(this);
			if (_childNode != null)
			{
				foreach (var child in _childNode)
				{
					child.GetAllChild(childList);
				}
			}
		}

	}
}
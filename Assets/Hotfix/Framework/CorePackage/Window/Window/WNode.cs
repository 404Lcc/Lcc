using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
	public abstract class WNode
	{
		/// <summary>
		/// 窗口的状态
		/// </summary>
		protected NodePhase _nodePhase;

		/// <summary>
		/// 是否被遮挡
		/// </summary>
		protected bool _isCovered;

		protected string _nodeName;

		protected string _logicName;

		//窗口逻辑
		protected IUILogic _logic;


		/// <summary>
		/// 全屏界面可以包含许多子界面
		/// </summary>
		protected List<WNode> _childNode;

		/// <summary>
		/// 关闭后会返回的界面
		/// </summary>
		public TurnNode returnNode;

		//根节点
		public WRootNode rootNode;

		//父节点
		public WNode parentNode;

		public bool newCreate = true;

		//是否全屏窗口
		public bool IsFullScreen;

		//回退类型
		public EscapeType escapeType;

		//释放窗口类型
		public ReleaseType releaseType = ReleaseType.AUTO;

		public int releaseTimer;



		//是否激活
		public bool Active => _nodePhase == NodePhase.ACTIVE;

		public string NodeName => _nodeName;


		public List<WNode> ChildNode => _childNode;



		public IUILogic Logic
		{
			get => _logic;
			set => _logic = value;
		}



		public string LogicName
		{
			get => _logicName;
			set => _logicName = value;
		}

		//判断是否包含节点（包含自身）
		public bool Contains(WNode node)
		{
			if (node == this)
				return true;

			if (_childNode == null)
				return false;

			foreach (WNode childNode in _childNode)
			{
				if (childNode.Contains(node))
				{
					return true;
				}
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

			if (_childNode == null || _childNode.Count == 0)
				return false;
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

			if (parentNode == null)
				return false;
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
			if (_childNode == null || _childNode.Count == 0)
				return this;
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
				if (parentNode != null && parentNode._nodePhase < NodePhase.ACTIVE)
					return;

				Log.Debug($"ui open window {NodeName}");

				newCreate = false;
				//把自己节点状态设置为激活
				_nodePhase = NodePhase.ACTIVE;
				//如果有父节点则把自己加进父级的子节点
				if (parentNode != null)
				{
					parentNode.ChildOpened(this);
				}

				DoOpen(param);

			}
		}

		public void Reset(object[] param)
		{
			if (_nodePhase >= NodePhase.ACTIVE)
			{
				DoReset(param);
			}
		}

		//父级调用打开孩子
		public void ChildOpened(WNode child)
		{
			if (_childNode == null)
			{
				_childNode = new List<WNode>();
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
					if (i < fullIndex)
					{
						_childNode[i].SetCovered(true);
					}
					else
					{
						_childNode[i].SetCovered(false);
					}
				}

			}

			DoChildOpened(child);
		}

		/// <summary>
		/// 设置覆盖
		/// </summary>
		/// <param name="covered"></param>
		public void SetCovered(bool covered)
		{
			if (_isCovered == covered)
				return;

			_isCovered = covered;

			if (covered)
			{
				Log.Debug($"ui pause window {NodeName}");
				DoCovered(covered);

				//给子节点全部暂停
				if (_childNode != null && _childNode.Count > 0)
				{
					for (int i = _childNode.Count - 1; i >= 0; i--)
					{
						_childNode[i].SetCovered(true);
					}
				}
			}
			else
			{
				if (parentNode != null && parentNode._isCovered)
					return;

				Log.Debug($"ui resume window {NodeName}");

				DoCovered(covered);

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
						_childNode[i].SetCovered(false);
					}
				}
			}
		}

		public object Close()
		{
			//如果是暂停状态
			if (_nodePhase == NodePhase.ACTIVE)
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

		/// <summary>
		/// 覆盖
		/// </summary>
		/// <param name="covered"></param>
		protected virtual void DoCovered(bool covered)
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

		//关闭所有子窗口
		public void CloseAllChild()
		{
			if (_childNode == null || _childNode.Count == 0)
				return;
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
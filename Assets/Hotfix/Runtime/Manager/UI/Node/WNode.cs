using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public bool Active => _nodePhase == NodePhase.ACTIVE;

        protected string _nodeName;
		public string NodeName => _nodeName;

		public WRootNode rootNode;

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
		
		public int nodeFlag { get; protected set; }
		public bool IsFullScreen
		{
			get { return (nodeFlag & (int) NodeFlag.FULL_SCREEN) > 0; }
		}

		public bool IsMainNode
		{
			get { return (nodeFlag & (int) NodeFlag.MAIN_NODE) > 0; }
		}
		public bool IsTopNode
		{
			get { return (nodeFlag & (int)NodeFlag.TOP_NODE) > 0; }
		}

		public EscapeType escapeType;

		public ReleaseType releaseType = ReleaseType.AUTO;

		public int releaseTimer;

		protected GameObject _gameObject;
		public GameObject gameObject
		{
			get => _gameObject;
			set => _gameObject = value;
		}

		protected Transform _transform;
		public Transform transform
		{
			get => _transform;
			set => _transform = value;
		}

		protected IUILogic _logic;
		public IUILogic logic
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

		public bool Contains(WNode node)
		{
			if (node == this) return true;
			if (_childNode == null) return false;
			foreach (WNode childNode in _childNode)
            {
				if( childNode.Contains(node))
					return true;
			}
			return false;
		}

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
				foreach(var child in parentNode._childNode)
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
				_nodePhase = NodePhase.OPENED;
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

		public void ChildOpened(WNode child)
		{
			if (_childNode == null)
				_childNode = new List<WNode>();
			// 检查节点标记
			if (_childNode != null && _childNode.Count > 0)
			{
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
					if ((_childNode[i].RejectFlag & child.RejectFlag) > 0)
					{
						_childNode[i].Close();
					}
				}
			}
			_childNode.Add(child);

			if (_childNode != null && _childNode.Count > 1)
			{
				int fullIndex = _childNode.Count;
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
					fullIndex = i;
					if (_childNode[i].IsFullScreen)
					{
						break;
					}
				}

				for (int i = _childNode.Count - 2; i >= 0; i--)
				{
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
			if (_nodePhase == NodePhase.OPENED)
			{
				if (parentNode != null && parentNode._nodePhase < NodePhase.ACTIVE) return;
				Log.Debug($"ui resume window {NodeName}");
				_nodePhase = NodePhase.ACTIVE;
				DoResume();
				if (_childNode != null && _childNode.Count > 0)
				{
					int fullIndex = _childNode.Count;
					for (int i = _childNode.Count - 1; i >= 0; i--)
					{
						fullIndex = i;
						if (_childNode[i].IsFullScreen)
						{
							break;
						}
					}

					for (int i = fullIndex; i < _childNode.Count; i++) 
					{
						_childNode[i].Resume();
					}
				}
			}
		}
		public void Pause()
        {
			if (_nodePhase == NodePhase.ACTIVE)
			{
				Log.Debug($"ui pause window {NodeName}");
				DoPause();
				_nodePhase = NodePhase.OPENED;
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
			if (_nodePhase == NodePhase.ACTIVE)
            {
				Pause();
			}
			if (_nodePhase == NodePhase.OPENED)
			{
				Log.Debug($"ui close window {NodeName}");
				// 由下向上
				if (parentNode != null)
				{
					parentNode.ChildClosed(this);
				}
                else if(this is WRootNode)
                {
                    Entry.GetModule<WindowManager>().RemoveRoot(this as WRootNode);
                }
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
		
		
		
		public bool ChildRequireEscape(WNode child)
        {
			if (DoChildRequireEscape(child))
            {
				child.Close();
				return true;
			}
			return false;
		}

		protected virtual void DoStart() { }
		protected virtual void DoUpdate() { }
		protected virtual void DoOpen(object[] param) { }
		protected virtual void DoReset(object[] param) { }
		protected virtual void DoResume() { }
		protected virtual void DoPause() { }
		protected virtual object DoClose() { return null; }
		protected virtual void DoRemove() { }
		protected virtual void DoSwitch(Action<bool> callback) { }
		protected virtual void DoChildOpened(WNode child) { }
		protected virtual void DoChildClosed(WNode child) { }

		/// <summary>
		/// 子节点请求推出
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		protected virtual bool DoChildRequireEscape(WNode child)
		{
			return true;
		}
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


		public bool TryCloseChild(string windowClose, out object val)
		{
			val = null;

			if (windowClose == NodeName)
			{
				val = Close();
				return true;
			}
			else
			{
				if (_childNode == null || _childNode.Count == 0) return false;
				for (int i = _childNode.Count - 1; i >= 0; i--)
				{
					if (_childNode[i].TryCloseChild(windowClose, out val))
						return true;
				}
			}
			
			return false;
		}

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

		public void CloseAllChild()
		{
			if (_childNode == null || _childNode.Count == 0) return;
			for (int i = _childNode.Count - 1; i >= 0; i--)
			{
				_childNode[i].Close();
			}
		}


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

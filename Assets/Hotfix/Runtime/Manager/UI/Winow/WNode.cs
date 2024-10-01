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
		    public string NodeName;
		    public NodeType NodeType;
		    public object[] NodeParam;
	    }
		
		/// <summary>
		/// 窗口的状态
		/// </summary>
		protected NodePhase nodePhase;
		public bool active { get { return nodePhase == NodePhase.ACTIVE; } }

		protected string w_nodeName;
		public string nodeName => w_nodeName;

		public WRootNode RootNode;

		public WNode ParentNode;

		/// <summary>
		/// 全屏界面可以包含许多子界面
		/// </summary>
		protected List<WNode> m_childNode;
		public List<WNode> childNode { get { return m_childNode; } }
		
		/// <summary>
		/// 关闭后会返回的界面
		/// </summary>
		public TurnNode returnNode;

		public bool newCreate = true;
		
		/// <summary>
		/// 用于处理两个窗口彼此互斥的情况
		/// 从属于同一窗口的子窗口flag的 & 运算结果不能大于0
		/// </summary>
		public int rejectFlag { get; protected set; }
		
		public int nodeFlag { get; protected set; }
		public bool isFullScreen
		{
			get { return (nodeFlag & (int) NodeFlag.FULL_SCREEN) > 0; }
		}

		public bool isMainNode
		{
			get { return (nodeFlag & (int) NodeFlag.MAIN_NODE) > 0; }
		}
		public bool isTopNode
		{
			get { return (nodeFlag & (int)NodeFlag.TOP_NODE) > 0; }
		}

		public EscapeType escapeType;

		public ReleaseType releaseType = ReleaseType.AUTO;

		public int releaseTimer;

		protected GameObject w_gameObject;
		public GameObject gameObject
		{
			get => w_gameObject;
			set => w_gameObject = value;
		}

		protected Transform w_transform;
		public Transform transform
		{
			get => w_transform;
			set => w_transform = value;
		}

		protected IUILogic w_logic;
		public IUILogic logic
		{
			get => w_logic;
			set => w_logic = value;
		}

		protected string w_logicName;
		public string logicName
		{
			get => w_logicName;
			set => w_logicName = value;
		}

		public bool Contains(WNode node)
		{
			if (node == this) return true;
			if (m_childNode == null) return false;
			foreach (WNode childNode in m_childNode)
            {
				if( childNode.Contains(node))
					return true;
			}
			return false;
		}

		public bool TryGetNode(string windowName, out WNode node)
		{
			node = null;
			if (w_nodeName.Equals(windowName)) 
            {
				node = this;
				return true;
            }
			if (m_childNode == null || m_childNode.Count == 0) return false;
			foreach (var childNode in m_childNode)
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

			if (ParentNode == null) return false;
			if (ParentNode.nodeName == windowName)
			{
				node = ParentNode;
				return true;
			}

			if (ParentNode.m_childNode != null && ParentNode.m_childNode.Count > 0)
			{
				foreach(var child in ParentNode.m_childNode)
				{
					if (child.nodeName == windowName)
					{
						node = ParentNode;
						return true;
					}
				}
			}

			return ParentNode.TryGetNodeForward(windowName, out node);
		}

		public WNode GetTopWindow()
		{
			if (m_childNode == null || m_childNode.Count == 0) return this;
			return m_childNode[m_childNode.Count - 1].GetTopWindow();
		}

		public void Start()
        {
			nodePhase = NodePhase.DEACTIVE;
			DoStart();
		}

		public void Update()
        {
			if (nodePhase == NodePhase.ACTIVE)
            {
				DoUpdate();
			}
		}
		public void Open(object[] param)
        {

			if (nodePhase == NodePhase.DEACTIVE)
            {
				if (ParentNode != null && ParentNode.nodePhase < NodePhase.OPENED) return;
				Log.Debug($"ui open window {nodeName}");
				newCreate = false;
				nodePhase = NodePhase.OPENED;
				if (ParentNode != null)
					ParentNode.ChildOpened(this);
				DoOpen(param);
				
			}
		}

		public void Reset(object[] param)
		{
			if (nodePhase >= NodePhase.OPENED)
			{
				DoReset(param);
			}
		}

		public void ChildOpened(WNode child)
		{
			if (m_childNode == null)
				m_childNode = new List<WNode>();
			// 检查节点标记
			if (m_childNode != null && m_childNode.Count > 0)
			{
				for (int i = m_childNode.Count - 1; i >= 0; i--)
				{
					if ((m_childNode[i].rejectFlag & child.rejectFlag) > 0)
					{
						m_childNode[i].Close();
					}
				}
			}
			m_childNode.Add(child);

			if (m_childNode != null && m_childNode.Count > 1)
			{
				int fullIndex = m_childNode.Count;
				for (int i = m_childNode.Count - 1; i >= 0; i--)
				{
					fullIndex = i;
					if (m_childNode[i].isFullScreen)
					{
						break;
					}
				}

				for (int i = m_childNode.Count - 2; i >= 0; i--)
				{
					if (i < fullIndex && !m_childNode[i].isTopNode) 
						m_childNode[i].Pause();
					else
						m_childNode[i].Resume();
				}

			}

			DoChildOpened(child);
		}

		public void Resume()
        {
			if (nodePhase == NodePhase.OPENED)
			{
				if (ParentNode != null && ParentNode.nodePhase < NodePhase.ACTIVE) return;
				Log.Debug($"ui resume window {nodeName}");
				nodePhase = NodePhase.ACTIVE;
				DoResume();
				if (m_childNode != null && m_childNode.Count > 0)
				{
					int fullIndex = m_childNode.Count;
					for (int i = m_childNode.Count - 1; i >= 0; i--)
					{
						fullIndex = i;
						if (m_childNode[i].isFullScreen)
						{
							break;
						}
					}

					for (int i = fullIndex; i < m_childNode.Count; i++) 
					{
						m_childNode[i].Resume();
					}
				}
			}
		}
		public void Pause()
        {
			if (nodePhase == NodePhase.ACTIVE)
			{
				Log.Debug($"ui pause window {nodeName}");
				DoPause();
				nodePhase = NodePhase.OPENED;
				if (m_childNode != null && m_childNode.Count > 0)
				{
					for (int i = m_childNode.Count - 1; i >= 0; i--)
					{
						m_childNode[i].Pause();
					}
				}
			}
		}

		public object Close()
        {
			if (nodePhase == NodePhase.ACTIVE)
            {
				Pause();
			}
			if (nodePhase == NodePhase.OPENED)
			{
				Log.Debug($"ui close window {nodeName}");
				// 由下向上
				if (ParentNode != null)
				{
					ParentNode.ChildClosed(this);
				}
                else if(this is WRootNode)
                {
                    Entry.GetModule<WindowManager>().RemoveRoot(this as WRootNode);
                }
				// 由上向下
				while (m_childNode != null && m_childNode.Count > 0)
				{
					var child = m_childNode[m_childNode.Count - 1];
					m_childNode.RemoveAt(m_childNode.Count - 1);
					child.ParentNode = null;
					child.Close();
				}
				m_childNode = null;
				returnNode = null;
				nodePhase = NodePhase.DEACTIVE;
				var returnValue = DoClose();

				return returnValue;
			}
			return null;
		}
		public void ChildClosed(WNode child)
		{
			if (m_childNode == null)
				return;
			m_childNode.Remove(child);
			child.ParentNode = null;
			child.RootNode = null;

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
			if (m_childNode != null && m_childNode.Count > 0)
			{
				for (int i = m_childNode.Count - 1; i >= 0; i--)
				{
					if (m_childNode[i].Escape(ref escape))
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
			if (escape == EscapeType.AUTO_CLOSE && ParentNode != null)
			{
				if (!ParentNode.ChildRequireEscape(this))
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

			if (windowClose == nodeName)
			{
				val = Close();
				return true;
			}
			else
			{
				if (m_childNode == null || m_childNode.Count == 0) return false;
				for (int i = m_childNode.Count - 1; i >= 0; i--)
				{
					if (m_childNode[i].TryCloseChild(windowClose, out val))
						return true;
				}
			}
			
			return false;
		}

		public void CloseChild(int flag)
		{
			if ((rejectFlag & flag) > 0)
			{
				Close();
			}
			if (m_childNode == null || m_childNode.Count == 0) return;
			for (int i = m_childNode.Count - 1; i >= 0; i--)
			{
				m_childNode[i].CloseChild(flag);
			}
		}

		public void CloseAllChild()
		{
			if (m_childNode == null || m_childNode.Count == 0) return;
			for (int i = m_childNode.Count - 1; i >= 0; i--)
			{
				m_childNode[i].Close();
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
			if (m_childNode != null)
            {
				foreach (var child in m_childNode)
				{
					child.GetAllChild(childList);
				}
			}
		}

	}
}

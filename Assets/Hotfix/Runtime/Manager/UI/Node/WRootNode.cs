using System;
using System.Collections.Generic;


namespace LccHotfix
{
    public class WRootNode : WNode
    {
        /// <summary>
        /// 在栈里的位置
        /// </summary>
        public int stackIndex;
        /// <summary>
        /// 每个根节点用于向下广播的黑板
        /// </summary>
        private WBlackboard w_blackboard;
        public WBlackboard Blackboard => w_blackboard;
        public WRootNode(string rootName)
        {
            this.w_nodeName = rootName;
            this.w_logicName = rootName;
            this.RootNode = this;
            this.escapeType = EscapeType.AUTO_CLOSE;
            this.releaseType = ReleaseType.AUTO;
            this.w_blackboard = new WBlackboard();
            this.w_logic = Entry.GetModule<WindowManager>().CreateLogic(rootName, null);
            if (w_logic != null)
                w_logic.wNode = this;
        }

        protected override void DoStart()
        {
            w_logic?.OnStart();
        }
        protected override void DoUpdate()
        {
            w_logic?.OnUpdate();
            w_blackboard?.Update();
        }
        protected override void DoSwitch(Action<bool> callback)
        {
            if (w_logic != null)
            {
                w_logic.OnSwitch(callback);
            }
            else
            {
                callback(true);
            }
        }
        protected override void DoOpen(object[] param)
        {
			gameObject?.SetActive(true);
			w_logic?.OnOpen(param);
        }
		protected override void DoReset(object[] param)
		{
			w_logic?.OnReset(param);
		}

		protected override void DoResume()
        {
            w_logic?.OnResume();
        }
        protected override void DoPause()
        {
            w_logic?.OnPause();
        }

        protected override object DoClose()
        {
			gameObject?.SetActive(false);
            object backValue = null;
            if (w_logic != null)
                backValue = w_logic.OnClose();
            Entry.GetModule<WindowManager>().OnWindowClose(nodeName, backValue);
            Entry.GetModule<WindowManager>().AddToReleaseQueue(this);
            return backValue;
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            if (w_logic != null)
                return w_logic.OnEscape(ref escape);
            return base.DoEscape(ref escape);
        }

        protected override void DoRemove()
        {
            w_logic?.OnRemove();
			if (gameObject != null)
				UnityEngine.Object.Destroy(gameObject);
		}
	    protected override void DoChildOpened(WNode child)
        {
            w_logic?.OnChildOpened(child);
        }
        protected override void DoChildClosed(WNode child)
        {
			if (active)
			{
				TurnNode turn = child.returnNode;

				if (turn != null)
				{

					if (!TryGetNodeForward(turn.NodeName, out WNode node))
					{
						switch (turn.NodeType)
						{
							case NodeType.ROOT:
                                Entry.GetModule<WindowManager>().OpenRoot(turn.NodeName, turn.NodeParam);
								break;
							case NodeType.WINDOW:
                                Entry.GetModule<WindowManager>().OpenWindow(turn.NodeName, turn.NodeParam);
								break;
						}
					}
				}
			}
			

			if (w_logic != null)
			{
                if (w_logic.OnChildClosed(child))
                    return;
			}
			else
			{
				if (DefaultChildCheck())
				{
                    Close();
                    return;
				}
			}


			if (active && child.isFullScreen)
			{
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

					if (fullIndex < m_childNode.Count)
					{
						for (int i = m_childNode.Count - 1; i >= fullIndex; i--)
						{
							m_childNode[i].Resume();
						}
					}
				}
			}
			
		}
        protected override bool DoChildRequireEscape(WNode child)
        {
            if (w_logic != null)
            {
                return w_logic.OnChildRequireEscape(child);
            }
            
            return true; 
        }

       

        /// <summary>
        /// 根据子节点的状态判断根节点是否需要关闭
        /// </summary>
        /// <returns></returns>
        public bool DefaultChildCheck()
		{
			if (m_childNode == null || m_childNode.Count == 0)
			{
				return true;
			}

			foreach (WNode wNode in m_childNode)
			{
				if (wNode.isMainNode)
				{
                    return false;
				}
			}

			return true;
		}


	}
}

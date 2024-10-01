using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace LccHotfix
{
    public class Window : WNode
    {

       
        /// <summary>
        /// window的配置数据
        /// </summary>
        private WindowMode w_mode;
        public WindowMode windowMode => w_mode;


        public Window(string windowName, WindowMode mode)
		{
            w_nodeName = windowName;
            w_mode = mode;
            rejectFlag = mode.rejectFlag;
            nodeFlag = mode.windowFlag;
            escapeType = (EscapeType) mode.escapeType;
			releaseType = (ReleaseType)mode.releaseType;
            w_logicName = mode.logicName;
		}
		
        protected override void DoStart()
        {
			w_logic.OnStart();
		}
		protected override void DoUpdate()
		{
			w_logic.OnUpdate();
		}
		protected override void DoSwitch(Action<bool> callback)
		{
			w_logic.OnSwitch(callback);
		}
		protected override void DoOpen(object[] param)
        {
			// 重置下返回节点
			if (!string.IsNullOrEmpty(windowMode.returnNodeName) && returnNode == null) 
			{
				returnNode = new WNode.TurnNode()
				{
					NodeName = windowMode.returnNodeName,
					NodeType = (NodeType)windowMode.returnNodeType,
				};
				if (windowMode.returnNodeParam >= 0)
					returnNode.NodeParam = new object[] { windowMode.returnNodeParam };
			}
			
			InternalOpen(true);
			w_logic.OnOpen(param);
		}
		protected override void DoReset(object[] param)
		{
			w_logic.OnReset(param);
		}

		protected override void DoResume()
        {
			InternalResume(true);
			if (windowMode.canShowLouder == 1)
			{
                Entry.GetModule<WindowManager>().commonRoot.Blackboard.Set(BlackboardType.UILouderSetDepth, new List<object>() { windowMode.depth + 100, windowMode.louderY });
			}
			else if (windowMode.canShowLouder == 0 && windowMode.windowFlag > 0)
			{
                Entry.GetModule<WindowManager>().commonRoot.Blackboard.Set(BlackboardType.UILouderSetDepth, new List<object>() { windowMode.depth - 100, windowMode.louderY });
			}
			w_logic.OnResume();
		}
        protected override void DoPause()
        {
			InternalResume(false);
			w_logic.OnPause();
		}

		protected override object DoClose()
		{
			InternalOpen(false);
			var backValue = w_logic.OnClose();
            Entry.GetModule<WindowManager>().OnWindowClose(nodeName, backValue);
            Entry.GetModule<WindowManager>().AddToReleaseQueue(this);
			return backValue;
		}
		protected override void DoChildClosed(WNode child)
		{
			if (RootNode.active)
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
		protected override bool DoEscape(ref EscapeType escape)
        {
			escape = this.escapeType;
			if (escape == EscapeType.SKIP_OVER)
				return false;
			if (!w_logic.OnEscape(ref escape))
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
		
		protected override bool DoChildRequireEscape(WNode child)
		{
			if (w_logic != null)
			{
				return w_logic.OnChildRequireEscape(child);
			}
			return true; 
		}

		protected override void DoRemove()
		{
			w_logic.OnRemove();
			if (gameObject != null)
				Object.Destroy(gameObject);
		}


		public void CreateWindowView()
		{
            w_gameObject = TDRes.LoadGameObject?.Invoke(w_mode.bundleName, w_mode.prefabName, true);
            if (w_gameObject != null)
			{
				w_transform = w_gameObject.transform;
			
				w_gameObject.SetActive(true);
			}
		}

		

		private void InternalOpen(bool enable)
		{
			gameObject?.SetActive(enable);
		}

		private void InternalResume(bool enable)
		{
			if (windowMode.canShowLouder == 1)
			{
				if (enable)
                    Entry.GetModule<WindowManager>().commonRoot.Blackboard.SetNum(BlackboardType.UILouderIsShow, 1);
				else
                    Entry.GetModule<WindowManager>().commonRoot.Blackboard.UnsetNum(BlackboardType.UILouderIsShow, 1);
			}

			TDUI.PauseWindowFunc?.Invoke(transform, enable);
			
			if (enable)
			{
				if (!string.IsNullOrEmpty(w_mode.bgTex))
					TDUI.RefreshBackgroundFunc?.Invoke(this, w_mode.bgTex);
				if (w_mode.sound > 0)
					TDUI.PlayWindowSound(w_mode.sound);
			}
		}
		
    }
}
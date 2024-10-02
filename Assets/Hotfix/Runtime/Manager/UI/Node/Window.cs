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
        private WindowMode _mode;
        public WindowMode windowMode => _mode;


        public Window(string windowName, WindowMode mode)
		{
            _nodeName = windowName;
            _mode = mode;
            RejectFlag = mode.rejectFlag;
            nodeFlag = mode.windowFlag;
            escapeType = (EscapeType) mode.escapeType;
			releaseType = (ReleaseType)mode.releaseType;
            _logicName = mode.logicName;
		}
		
        protected override void DoStart()
        {
			_logic.OnStart();
		}
		protected override void DoUpdate()
		{
			_logic.OnUpdate();
		}
		protected override void DoSwitch(Action<bool> callback)
		{
			_logic.OnSwitch(callback);
		}
		protected override void DoOpen(object[] param)
        {
			// 重置下返回节点
			if (!string.IsNullOrEmpty(windowMode.returnNodeName) && returnNode == null) 
			{
				returnNode = new WNode.TurnNode()
				{
					nodeName = windowMode.returnNodeName,
					nodeType = (NodeType)windowMode.returnNodeType,
				};
				if (windowMode.returnNodeParam >= 0)
					returnNode.nodeParam = new object[] { windowMode.returnNodeParam };
			}
			
			InternalOpen(true);
			_logic.OnOpen(param);
		}
		protected override void DoReset(object[] param)
		{
			_logic.OnReset(param);
		}

		protected override void DoResume()
        {
			InternalResume(true);
			if (windowMode.canShowLouder == 1)
			{
                Entry.GetModule<WindowManager>().CommonRoot.Blackboard.Set(BlackboardType.UILouderSetDepth, new List<object>() { windowMode.depth + 100, windowMode.louderY });
			}
			else if (windowMode.canShowLouder == 0 && windowMode.windowFlag > 0)
			{
                Entry.GetModule<WindowManager>().CommonRoot.Blackboard.Set(BlackboardType.UILouderSetDepth, new List<object>() { windowMode.depth - 100, windowMode.louderY });
			}
			_logic.OnResume();
		}
        protected override void DoPause()
        {
			InternalResume(false);
			_logic.OnPause();
		}

		protected override object DoClose()
		{
			InternalOpen(false);
			var backValue = _logic.OnClose();
            Entry.GetModule<WindowManager>().OnWindowClose(NodeName, backValue);
            Entry.GetModule<WindowManager>().AddToReleaseQueue(this);
			return backValue;
		}
		protected override void DoChildClosed(WNode child)
		{
			if (rootNode.Active)
			{
				TurnNode turn = child.returnNode;

				if (turn != null)
				{

					if (!TryGetNodeForward(turn.nodeName, out WNode node))
					{
						switch (turn.nodeType)
						{
							case NodeType.ROOT:
                                Entry.GetModule<WindowManager>().OpenRoot(turn.nodeName, turn.nodeParam);
								break;
							case NodeType.WINDOW:
                                Entry.GetModule<WindowManager>().OpenWindow(turn.nodeName, turn.nodeParam);
								break;
						}
					}
				}
			}
			
			
			if (Active && child.IsFullScreen)
			{
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

					if (fullIndex < _childNode.Count)
					{
						for (int i = _childNode.Count - 1; i >= fullIndex; i--)
						{
							_childNode[i].Resume();
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
			if (!_logic.OnEscape(ref escape))
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
		
		protected override bool DoChildRequireEscape(WNode child)
		{
			if (_logic != null)
			{
				return _logic.OnChildRequireEscape(child);
			}
			return true; 
		}

		protected override void DoRemove()
		{
			_logic.OnRemove();
			if (gameObject != null)
				Object.Destroy(gameObject);
		}


		public void CreateWindowView()
		{
            _gameObject = TDRes.LoadGameObject?.Invoke(_mode.bundleName, _mode.prefabName, true);
            if (_gameObject != null)
			{
				_transform = _gameObject.transform;
			
				_gameObject.SetActive(true);
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
                    Entry.GetModule<WindowManager>().CommonRoot.Blackboard.SetNum(BlackboardType.UILouderIsShow, 1);
				else
                    Entry.GetModule<WindowManager>().CommonRoot.Blackboard.UnsetNum(BlackboardType.UILouderIsShow, 1);
			}

			TDUI.PauseWindowFunc?.Invoke(transform, enable);
			
			if (enable)
			{
				if (!string.IsNullOrEmpty(_mode.bgTex))
					TDUI.RefreshBackgroundFunc?.Invoke(this, _mode.bgTex);
				if (_mode.sound > 0)
					TDUI.PlayWindowSound(_mode.sound);
			}
		}
		
    }
}
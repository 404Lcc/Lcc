using System;
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
		public WindowMode WindowMode => _mode;


		public Window(string windowName, WindowMode mode)
		{
			_nodeName = windowName;
			_mode = mode;
			RejectFlag = mode.rejectFlag;
			NodeFlag = mode.windowFlag;
			escapeType = (EscapeType)mode.escapeType;
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
			if (!string.IsNullOrEmpty(WindowMode.returnNodeName) && returnNode == null)
			{
				returnNode = new WNode.TurnNode()
				{
					nodeName = WindowMode.returnNodeName,
					nodeType = (NodeType)WindowMode.returnNodeType,
				};
				if (WindowMode.returnNodeParam >= 0)
					returnNode.nodeParam = new object[] { WindowMode.returnNodeParam };
			}

			//内部打开
			InternalOpen(true);
			_logic.OnOpen(param);
		}
		protected override void DoReset(object[] param)
		{
			_logic.OnReset(param);
		}

		protected override void DoResume()
		{
			//内部恢复
			InternalResume(true);

			_logic.OnResume();
		}
		protected override void DoPause()
		{
			//内部暂停
			InternalResume(false);
			_logic.OnPause();
		}

		protected override object DoClose()
		{
			//内部关闭
			InternalOpen(false);
			var backValue = _logic.OnClose();
            //触发关闭节点回调
            Entry.GetModule<WindowManager>().OnWindowClose(NodeName, backValue);
            //加入到释放列表
            Entry.GetModule<WindowManager>().AddToReleaseQueue(this);
			return backValue;
		}
		protected override void DoChildClosed(WNode child)
		{
            //如果根节点激活
            if (rootNode.Active)
			{
				TurnNode turn = child.returnNode;

                //如果有关闭后返回窗口，尝试打开
                if (turn != null)
				{
                    //如果没有父节点，尝试根据类型打开窗口
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

            //如果当前节点激活并且关闭的子节点是全屏窗口
            //这个时候_childNode里已经没有要移除的child了
            if (Active && child.IsFullScreen)
			{
				if (_childNode != null && _childNode.Count > 0)
				{
                    //找到最新的全屏窗口索引
                    int fullIndex = _childNode.Count;
					for (int i = _childNode.Count - 1; i >= 0; i--)
					{
						fullIndex = i;
						if (_childNode[i].IsFullScreen)
						{
							break;
						}
					}

                    //找到全屏窗口后面的节点，包含这个全屏窗口
                    if (fullIndex < _childNode.Count)
					{
                        //恢复全屏界面和后面的节点（假如_childNode.count是10个节点，fullIndex是5，则恢复5到9）
                        for (int i = _childNode.Count - 1; i >= fullIndex; i--)
						{
							_childNode[i].Resume();
						}
					}
				}
			}
		}
		//处理窗口返回
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

        //子节点请求退出
        protected override bool DoChildRequireEscape(WNode child)
		{
			if (_logic != null)
			{
				return _logic.OnChildRequireEscape(child);
			}
			return true;
		}
		//移除
		protected override void DoRemove()
		{
			_logic.OnRemove();
			if (gameObject != null)
				Object.Destroy(gameObject);
		}

		//创建窗口
		public void CreateWindowView()
		{
			_gameObject = Entry.GetModule<WindowManager>().LoadGameObject?.Invoke(_mode.prefabName);
			if (_gameObject != null)
			{
				_transform = _gameObject.transform as RectTransform;

				_gameObject.SetActive(true);
			}
		}


		//内部打开关闭
		private void InternalOpen(bool enable)
		{
			gameObject?.SetActive(enable);
		}
		//内部恢复暂停
		private void InternalResume(bool enable)
		{
			Entry.GetModule<WindowManager>().PauseWindowFunc?.Invoke(transform, enable);

			if (enable)
			{
				if (!string.IsNullOrEmpty(_mode.bgTex))
					Entry.GetModule<WindowManager>().RefreshBackgroundFunc?.Invoke(this, _mode.bgTex);
				if (_mode.sound > 0)
					Entry.GetModule<WindowManager>().PlayWindowSoundFunc?.Invoke(_mode.sound);
			}
		}

	}
}
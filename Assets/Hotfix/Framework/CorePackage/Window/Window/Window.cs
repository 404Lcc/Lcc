using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
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

	public class Window : WNode
	{
		protected WindowMode _mode;
		protected GameObject _gameObject;
		protected RectTransform _transform;

		public bool IsFullScreen { get; protected set; }

		public WindowMode WindowMode => _mode;
		public GameObject gameObject => _gameObject;
		public RectTransform transform => _transform;

		public Window(string windowName, WindowMode mode)
		{
			_nodeName = windowName;
			_mode = mode;
			IsFullScreen = true; //todo 11.29
			escapeType = mode.escapeType;
			releaseType = mode.releaseType;
			_logicName = mode.logicName;
		}

		#region 必要流程

		public override void SetCovered(bool covered)
		{
			if (IsCovered == covered)
				return;

			IsCovered = covered;

			if (covered)
			{
				Log.Debug($"ui pause window {NodeName}");
				DoCovered(covered);

			}
			else
			{
				if (rootNode != null && rootNode.IsCovered)
					return;

				Log.Debug($"ui resume window {NodeName}");

				DoCovered(covered);
			}
		}

		public override void Open(object[] param)
		{
			if (NodePhase == NodePhase.DEACTIVE)
			{
				if (rootNode != null && rootNode.NodePhase < NodePhase.ACTIVE)
					return;

				Log.Debug($"ui open window {NodeName}");

				//把自己节点状态设置为激活
				NodePhase = NodePhase.ACTIVE;
				//如果有父节点则把自己加进父级的子节点
				if (rootNode != null)
				{
					rootNode.ChildOpened(this);
				}

				DoOpen(param);
			}
		}

		/// <summary>
		/// 返回键请求关闭窗口处理
		/// </summary>
		/// <param name="escape"></param>
		/// <returns></returns>
		public override bool Escape(ref EscapeType escape)
		{
			// 处理自己的返回
			return DoEscape(ref escape);
		}

		public override object Close()
		{
			//如果是暂停状态
			if (NodePhase == NodePhase.ACTIVE)
			{
				Log.Debug($"ui close window {NodeName}");
				//如果有父级
				// 由下向上
				if (rootNode != null)
				{
					//移除从父级移除当前节点
					rootNode.ChildClosed(this);
				}

				returnNode = null;
				//设置关闭状态
				NodePhase = NodePhase.DEACTIVE;
				var returnValue = DoClose();

				return returnValue;
			}

			return null;
		}

		#endregion

		#region 接口

		protected override void DoStart()
		{
			_logic.OnStart();
		}

		protected override void DoSwitch(Action<bool> callback)
		{
			_logic.OnSwitch(callback);
		}

		protected override void DoCovered(bool covered)
		{
			if (covered)
			{
				gameObject?.SetActive(false);
			}
			else
			{
				gameObject?.SetActive(true);
			}

			_logic.DoCovered(covered);
		}

		protected override void DoOpen(object[] param)
		{
			// 重置下返回节点
			if (!string.IsNullOrEmpty(WindowMode.returnNodeName) && returnNode == null)
			{
				returnNode = new TurnNode()
				{
					nodeName = WindowMode.returnNodeName,
					nodeType = (NodeType)WindowMode.returnNodeType,
				};
				if (WindowMode.returnNodeParam >= 0)
				{
					returnNode.nodeParam = new object[] { WindowMode.returnNodeParam };
				}
			}

			//内部打开
			gameObject?.SetActive(true);

			_logic.OnOpen(param);
		}

		protected override void DoReset(object[] param)
		{
			_logic.OnReset(param);
		}

		protected override void DoUpdate()
		{
			_logic.OnUpdate();
		}

		protected override object DoClose()
		{
			//内部关闭
			gameObject?.SetActive(false);

			var backValue = _logic.OnClose();
			//触发关闭节点回调
			Main.WindowService.OnWindowClose(NodeName, backValue);
			//加入到释放列表
			Main.WindowService.AddToReleaseQueue(this);
			return backValue;
		}

		//移除
		protected override void DoRemove()
		{
			_logic.OnRemove();
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
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
			if (escape == EscapeType.AUTO_CLOSE && rootNode != null)
			{
				if (!rootNode.ChildRequireEscape(this))
				{
					escape = EscapeType.REFUSE_AND_BREAK;
					return false;
				}
			}

			return true;
		}

		#endregion

		public void CreateWindowView(AssetLoader loader, Action<Window> callback)
		{
			Main.WindowService.LoadAsyncGameObject?.Invoke(loader, _mode.prefabName, (obj) =>
			{
				_gameObject = GameObject.Instantiate(obj);
				_gameObject.name = _mode.prefabName;
				if (_gameObject != null)
				{
					_transform = _gameObject.transform as RectTransform;

					_gameObject.SetActive(true);
				}

				callback?.Invoke(this);
			});
		}
	}
}
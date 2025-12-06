using System;

namespace LccHotfix
{
	public abstract class WNode
	{
		protected string _nodeName;

		protected string _logicName;

		//窗口逻辑
		protected IUILogic _logic;

		//根节点
		public WRootNode rootNode;


		//回退类型
		public EscapeType escapeType;

		//释放窗口类型
		public ReleaseType releaseType = ReleaseType.AUTO;

		public int releaseTimer;


		/// <summary>
		/// 是否被遮挡
		/// </summary>
		public bool IsCovered { get; protected set; }

		/// <summary>
		/// 窗口的状态
		/// </summary>
		public NodePhase NodePhase { get; protected set; }

		//是否激活
		public bool Active => NodePhase == NodePhase.ACTIVE;

		public string NodeName => _nodeName;


		public IUILogic Logic
		{
			get => _logic;
			set => _logic = value;
		}

		public string LogicName => _logicName;


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

		#region 必要流程

		public void Start()
		{
			NodePhase = NodePhase.DEACTIVE;
			DoStart();
		}

		public void Switch(Action<bool> callback)
		{
			DoSwitch(callback);
		}

		public abstract void SetCovered(bool covered);

		public abstract void Open(object[] param);

		public void Reset(object[] param)
		{
			if (NodePhase == NodePhase.ACTIVE)
			{
				DoReset(param);
			}
		}

		public void Update()
		{
			if (NodePhase == NodePhase.ACTIVE)
			{
				DoUpdate();
			}
		}

		/// <summary>
		/// 返回键请求关闭窗口处理
		/// </summary>
		/// <param name="escape"></param>
		/// <returns></returns>
		public abstract bool Escape(ref EscapeType escape);

		public abstract object Close();

		/// <summary>
		/// 从内存中移除
		/// </summary>
		public void Remove()
		{
			DoRemove();
		}


		#endregion

		#region 接口


		//开始
		protected abstract void DoStart();

		//切换窗口
		protected abstract void DoSwitch(Action<bool> callback);

		//覆盖
		protected abstract void DoCovered(bool covered);

		//打开
		protected abstract void DoOpen(object[] param);

		//重置
		protected abstract void DoReset(object[] param);

		//更新
		protected abstract void DoUpdate();

		//关闭
		protected abstract object DoClose();

		//移除（彻底关闭）
		protected abstract void DoRemove();

		//处理窗口返回
		protected abstract bool DoEscape(ref EscapeType escape);

		#endregion
	}
}
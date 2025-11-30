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
		public NodePhase _nodePhase;

		/// <summary>
		/// 是否被遮挡
		/// </summary>
		public bool _isCovered;

		protected string _nodeName;

		protected string _logicName;

		//窗口逻辑
		protected IUILogic _logic;




		/// <summary>
		/// 关闭后会返回的界面
		/// </summary>
		public TurnNode returnNode;

		//根节点
		public WRootNode rootNode;

		// //父节点
		// public WRootNode parentNode;


		//回退类型
		public EscapeType escapeType;

		//释放窗口类型
		public ReleaseType releaseType = ReleaseType.AUTO;

		public int releaseTimer;



		//是否激活
		public bool Active => _nodePhase == NodePhase.ACTIVE;

		public string NodeName => _nodeName;


	



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

		public virtual void Open(object[] param)
		{

		}

		public void Reset(object[] param)
		{
			if (_nodePhase >= NodePhase.ACTIVE)
			{
				DoReset(param);
			}
		}




		/// <summary>
		/// 设置覆盖
		/// </summary>
		/// <param name="covered"></param>
		public virtual void SetCovered(bool covered)
		{
		
		}

		public virtual object Close()
		{
			return null;
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
		public virtual bool Escape(ref EscapeType escape)
		{
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
		





		//处理窗口返回
		protected virtual bool DoEscape(ref EscapeType escape)
		{
			return false;
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




	}
}
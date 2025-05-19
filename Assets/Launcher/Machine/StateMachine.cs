using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace LccModel
{
	public class StateMachine
	{
		private readonly Dictionary<string, Object> _blackboard = new Dictionary<string, object>(100);
		private readonly Dictionary<string, IStateNode> _nodes = new Dictionary<string, IStateNode>(100);
		private IStateNode _curNode;
		private IStateNode _preNode;

		/// <summary>
		/// 状态机持有者
		/// </summary>
		public Object Owner { private set; get; }

		/// <summary>
		/// 当前运行的节点名称
		/// </summary>
		public string CurrentNode
		{
			get { return _curNode != null ? _curNode.GetType().FullName : string.Empty; }
		}

		/// <summary>
		/// 之前运行的节点名称
		/// </summary>
		public string PreviousNode
		{
			get { return _preNode != null ? _preNode.GetType().FullName : string.Empty; }
		}


		private StateMachine() { }
		public StateMachine(Object owner)
		{
			Owner = owner;
		}

		/// <summary>
		/// 更新状态机
		/// </summary>
		public void Update()
		{
			if (_curNode != null)
				_curNode.OnUpdate();
		}

		/// <summary>
		/// 启动状态机
		/// </summary>
		public void Run<TNode>() where TNode : IStateNode
		{
			var nodeType = typeof(TNode);
			var nodeName = nodeType.FullName;
			Run(nodeName);
		}
		public void Run(Type entryNode)
		{
			var nodeName = entryNode.FullName;
			Run(nodeName);
		}
		public void Run(string entryNode)
		{
			_curNode = TryGetNode(entryNode);
			_preNode = _curNode;

			if (_curNode == null)
				throw new Exception($"Not found entry node: {entryNode}");

			_curNode.OnEnter();
		}

		/// <summary>
		/// 加入一个节点
		/// </summary>
		public void AddNode<TNode>() where TNode : IStateNode
		{
			var nodeType = typeof(TNode);
			var stateNode = Activator.CreateInstance(nodeType) as IStateNode;
			AddNode(stateNode);
		}
		public void AddNode(IStateNode stateNode)
		{
			if (stateNode == null)
				throw new ArgumentNullException();

			var nodeType = stateNode.GetType();
			var nodeName = nodeType.FullName;

			if (_nodes.ContainsKey(nodeName) == false)
			{
				stateNode.OnCreate(this);
				_nodes.Add(nodeName, stateNode);
			}
			else
			{
				Debug.LogError($"State node already existed : {nodeName}");
			}
		}

		/// <summary>
		/// 转换状态节点
		/// </summary>
		public void ChangeState<TNode>() where TNode : IStateNode
		{
			var nodeType = typeof(TNode);
			var nodeName = nodeType.FullName;
			ChangeState(nodeName);
		}
		public void ChangeState(Type nodeType)
		{
			var nodeName = nodeType.FullName;
			ChangeState(nodeName);
		}
		public void ChangeState(string nodeName)
		{
			if (string.IsNullOrEmpty(nodeName))
				throw new ArgumentNullException();

			IStateNode node = TryGetNode(nodeName);
			if (node == null)
			{
				Debug.LogError($"Can not found state node : {nodeName}");
				return;
			}

			Debug.Log($"{_curNode.GetType().FullName} --> {node.GetType().FullName}");
			_preNode = _curNode;
			_curNode.OnExit();
			_curNode = node;
			_curNode.OnEnter();
		}

		/// <summary>
		/// 设置黑板数据
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetBlackboardValue(string key, Object value)
		{
			if (_blackboard.ContainsKey(key) == false)
				_blackboard.Add(key, value);
			else
				_blackboard[key] = value;
		}

		/// <summary>
		/// 判断黑板是否存在某值
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsBlackboardValue(string key)
		{
			return _blackboard.ContainsKey(key);
		}
		
		/// <summary>
		/// 获取黑板数据
		/// </summary>
		public Object GetBlackboardValue(string key)
		{
			if (ContainsBlackboardValue(key))
			{
				return _blackboard[key];
			}
			
			return null;
		}

		private IStateNode TryGetNode(string nodeName)
		{
			_nodes.TryGetValue(nodeName, out IStateNode result);
			return result;
		}
	}
}
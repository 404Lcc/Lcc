using System;
using System.Collections.Generic;
using Object = System.Object;

namespace LccHotfix
{
    public interface IGuideStateNode
    {
        void OnCreate(GuideFSM machine);
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    public class GuideFSM
    {
        private GuideStateData _data;
        private string _defaultState;

        private readonly Dictionary<string, Object> _blackboard = new Dictionary<string, object>(100);
        private readonly Dictionary<string, IGuideStateNode> _nodes = new Dictionary<string, IGuideStateNode>(100);
        private IGuideStateNode _curNode;
        private IGuideStateNode _preNode;

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

        public GuideFSM(GuideStateData data)
        {
            _data = data;
        }

        public void SetDefaultState(string name)
        {
            _defaultState = name;
        }

        public void Reset()
        {
            _preNode = null;
            if (_curNode != null)
            {
                _curNode.OnExit();
                _curNode = null;
            }

            _data.Reset();
        }

        public void Release()
        {
            if (_curNode != null)
            {
                _curNode.OnExit();
            }

            _blackboard.Clear();
            _nodes.Clear();

            _data.Reset();

            _curNode = null;
            _preNode = null;
            _data = null;
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {
            if (_curNode != null)
            {
                _curNode.OnUpdate();
            }
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        public void Run<TNode>() where TNode : IGuideStateNode
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

        public void RunDefault()
        {
            if (string.IsNullOrEmpty(_defaultState))
                return;
            Run(_defaultState);
            _data.IsRunning = true;
        }

        /// <summary>
        /// 加入一个节点
        /// </summary>
        public void AddNode<TNode>() where TNode : IGuideStateNode
        {
            var nodeType = typeof(TNode);
            var stateNode = Activator.CreateInstance(nodeType) as IGuideStateNode;
            AddNode(stateNode);
        }

        public void AddNode(IGuideStateNode stateNode)
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
                UnityEngine.Debug.LogError($"State node already existed : {nodeName}");
            }
        }

        /// <summary>
        /// 转换状态节点
        /// </summary>
        public void ChangeState<TNode>() where TNode : IGuideStateNode
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

            IGuideStateNode node = TryGetNode(nodeName);
            if (node == null)
            {
                UnityEngine.Debug.LogError($"Can not found state node : {nodeName}");
                return;
            }

            UnityEngine.Debug.Log($"{_curNode.GetType().FullName} --> {node.GetType().FullName}");
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

        private IGuideStateNode TryGetNode(string nodeName)
        {
            _nodes.TryGetValue(nodeName, out IGuideStateNode result);
            return result;
        }
    }
}
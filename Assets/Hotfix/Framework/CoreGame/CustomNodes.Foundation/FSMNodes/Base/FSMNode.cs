using System.Collections.Generic;

namespace LccHotfix
{
    public interface IFSMNodeCfg
    {
        List<ICustomNodeCfg> StateList { get; }
        string DefaultState { get; }
        int MaxTransitionInOneFrame { get; }
        List<StateTransitionNodeCfg> GlobalTransitions { get; }
        string RecordCurStateIDTo { get; }
        public System.Type FsmImpType { get; }

        //FSM支持较为灵活的模版，这里缓存一下，运行时解析过模版后的静态配置
        public List<ICustomNodeCfg> CachedStateCfgList { get; set; }
    }

    //静态配置
    public class FSMNodeCfg : IFSMNodeCfg, ICustomNodeCfg
    {
        public List<ICustomNodeCfg> StateList { get; set; }

        public string DefaultState { get; set; } = null;

        public int MaxTransitionInOneFrame { get; set; } = 1; //同一帧内最多能发生多少次状态切换

        public List<StateTransitionNodeCfg> GlobalTransitions { get; set; } = null; //跳转逻辑（可以不配）

        public string RecordCurStateIDTo { get; set; } = null; //切换时记录当前状态到黑板

        //因优化添加, 属于内部实现细节，不要直接配置使用！！！
        public List<ICustomNodeCfg> CachedStateCfgList { get; set; } = null;

        public System.Type FsmImpType { get; set; }

        public virtual System.Type NodeType()
        {
            return FsmImpType ?? typeof(FSMNode);
        }

        public FSMNodeCfg WithRecordCurStateIDTo(string key)
        {
            RecordCurStateIDTo = key;
            return this;
        }
    }

    /// <summary>
    /// 运行时节点: 状态机
    /// </summary>
    public class FSMNode : CustomNode, INeedUpdate, INeedStopCheck
    {
        private IFSMNodeCfg _cfg;
        protected Dictionary<string, StateNode> _states = new();
        private string _defaultStateID;
        private StateNode _currentState;

        protected List<StateTransitionNode> _transitions = new();


        private HashSet<int> _usedTempLogicSet = new();
        private Dictionary<string, ICustomNodeCfg> _tempStateCfgs = new(16);

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);

            var theCfg = cfg as IFSMNodeCfg;
            _defaultStateID = theCfg.DefaultState;
            _states.Clear();

            if (theCfg.CachedStateCfgList == null)
            {
                _usedTempLogicSet.Clear();
                _tempStateCfgs.Clear();
                Inner_InitializeStates(theCfg, context, _usedTempLogicSet);
                theCfg.CachedStateCfgList = new(_tempStateCfgs.Count);
                foreach (var item in _tempStateCfgs)
                {
                    theCfg.CachedStateCfgList.Add(item.Value);
                }
            }

            foreach (var stateCfg in theCfg.CachedStateCfgList)
            {
                StateNode stateNode = _context.Factory.CreateCustomNode(stateCfg, context) as StateNode;
                stateNode.Deactivate();
                _states.Add(stateNode.StateID, stateNode);
            }

            if (theCfg.GlobalTransitions != null)
            {
                for (int i = 0; i < theCfg.GlobalTransitions.Count; ++i)
                {
                    ICustomNodeCfg bhvCfg = theCfg.GlobalTransitions[i];
                    var transNode = _context.Factory.CreateCustomNode(bhvCfg, context) as StateTransitionNode;
                    if (!CLHelper.Assert(transNode != null))
                        continue;
                    _transitions.Add(transNode);
                }
            }

            _cfg = theCfg;
            _currentState = null;
            CLHelper.Assert(_states.Count > 0);
        }

        protected void Inner_InitializeStates(IFSMNodeCfg fsmCfg, CustomNodeContext context, HashSet<int> usedTempLogicSet)
        {
            for (int i = 0; i < fsmCfg.StateList.Count; i++)
            {
                var nodeCfg = fsmCfg.StateList[i];

                //////////////////////////// 处理模板引用 Begin ////////////////////////////
                if (nodeCfg is LogicTempleteCfg templeteCfg)
                {
                    int templeteID = templeteCfg.LogicID;
                    if (usedTempLogicSet.Contains(templeteID))
                    {
                        this.LogError($"ERROR: 循环引用 FSMNode 模板! templeteID={templeteID}");
                        continue;
                    }

                    usedTempLogicSet.Add(templeteID);
                    var cfgContainer = context.ConfigContainer;
                    if (cfgContainer == null)
                    {
                        this.LogError($"ERROR: FSMNode 找不到设定模板库！cfgContainer != null, templeteID={templeteID}");
                        continue;
                    }

                    var templeteLogicCfg = cfgContainer.GetCustomLogicCfg(templeteID);
                    if (templeteLogicCfg != null)
                    {
                        //插入模板CustomLogic所配置的各个节点
                        foreach (var templeteNodeCfg in templeteLogicCfg.GetNodeCfgList())
                        {
                            if (templeteNodeCfg is FSMNodeCfg templeteFsmCfg)
                            {
                                Inner_InitializeStates(templeteFsmCfg, context, usedTempLogicSet);
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.LogError($"ERROR: FSMNode 找不到模板! templeteID={templeteID}");
                    }

                    continue;
                }
                ////////////////////////////// 处理模板引用 End ////////////////////////////

                if (nodeCfg is StateNodeCfg stateNodeCfg)
                {
                    Inner_CreateStateNode(context, stateNodeCfg);
                }
                else
                {
                    this.LogError($"Inner_CreateStateNode stateNodeCfg == null, RootLogicID={context.GenInfo.LogicConfigID}, idx={i}");
                }
            }
        }

        protected void Inner_CreateStateNode(CustomNodeContext context, StateNodeCfg stateNodeCfg)
        {
            var stateID = stateNodeCfg.StateID;
            _tempStateCfgs[stateID] = stateNodeCfg;
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            base.CollectInterfaceInChildren<T>(ref interfaceList);
            foreach (var item in _states)
            {
                TraverseCollectInterface(ref interfaceList, item.Value);
            }

            foreach (var transNode in _transitions)
            {
                TraverseCollectInterface(ref interfaceList, transNode);
            }
        }

        public override void Activate()
        {
            base.Activate();
            if (_currentState != null)
            {
                _currentState.Activate();
            }

            foreach (var transNode in _transitions)
            {
                transNode.Activate();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            if (_currentState != null && _currentState.IsActive)
            {
                _currentState.Deactivate();
            }

            foreach (var transNode in _transitions)
            {
                transNode.Deactivate();
            }
        }

        public override void Destroy()
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }

            foreach (var item in _states)
            {
                _context.Factory.DestroyCustomNode(item.Value);
            }

            _states.Clear();

            foreach (var transNode in _transitions)
            {
                _context.Factory.DestroyCustomNode(transNode);
            }

            _transitions.Clear();

            _currentState = null;
            _defaultStateID = null;
            _cfg = null;

            base.Destroy();
        }


        public virtual bool CanStop()
        {
            if (_states.Count == 0)
                return true;
            return false;
        }


        public virtual float Update(float dt)
        {
            if (_states.Count == 0)
                return dt;

            if (_currentState == null)
            {
                TransToState(_defaultStateID);
            }

            //先检查全局的Transitions
            foreach (var transNode in _transitions)
            {
                var transGoalStateID = transNode.CheckTransitions();
                if (transGoalStateID != null)
                {
                    if (_currentState.StateID != transGoalStateID)
                    {
                        TransToState(transGoalStateID);
                    }
                }
            }

            if (_currentState == null)
            {
                return dt;
            }

            //支持一帧内连续切换 MaxTransitionInOneFrame 个状态
            for (int i = 0; i < _cfg.MaxTransitionInOneFrame; i++)
            {
                //检查状态转移
                var oldStateID = _currentState.StateID;
                var goalStateID = _currentState.CheckTransitions();
                if (goalStateID == oldStateID)
                {
                    break; //如果没有状态转移
                }

                var goalState = FindState(goalStateID);
                if (goalState != null)
                {
                    _currentState.Exit();
                    _currentState = goalState;
                    _currentState.Enter();
                }
            }

            return _currentState.Update(dt);
        }

        public StateNode CurrentState
        {
            get { return _currentState; }
        }

        public string CurrentStateID
        {
            get
            {
                if (_currentState == null)
                    return null;
                return _currentState.StateID;
            }
        }

        public void TransToState(string goalStateID)
        {
            if (goalStateID == null)
                return;
            var goalState = FindState(goalStateID);
            if (goalState == null)
            {
                this.LogError($"FSMNode:FN_TransToState mGoalState == null  goalStateID={goalStateID}");
                return;
            }

            _currentState?.Exit();

            _currentState = goalState;
            _currentState.Enter();

            //记录当前状态到黑板
            if (!string.IsNullOrEmpty(_cfg.RecordCurStateIDTo))
            {
                SetVar(_cfg.RecordCurStateIDTo, goalStateID);
            }
        }


        private StateNode FindState(string stateID)
        {
            if (stateID == null)
            {
                return null;
            }

            if (_states.TryGetValue(stateID, out var state))
            {
                return state;
            }

            return null;
        }

        public override void Reset()
        {
            foreach (var transNode in _transitions)
            {
                transNode.Reset();
            }

            if (_currentState != null)
                _currentState.Exit();

            _currentState = FindState(_defaultStateID);

            if (_currentState != null)
                _currentState.Enter();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LccHotfix
{
    //////////////////////////////////////////////////////////////////////////
    // 配置语法糖:
    // 收纳到这里的都是任意项目通用的，绝对可以跨项目使用
    // 项目特化的不要放这里 ！！！！！
    //////////////////////////////////////////////////////////////////////////
    public partial class LogicConfigBase : Dictionary<int, CustomLogicCfg>, ILogicConfigContainer
    {
        protected Type DefaultLogicType;
        public string ContainerName { get; private set; }

        public LogicConfigBase(string name, int capacity = 64) : base(capacity)
        {
            ContainerName = name;
            DefaultLogicType = typeof(CustomLogic);
        }

        protected CustomLogicCfg AddConfig(int id, List<ICustomNodeCfg> nodes, string desc = null)
        {
            var logicCfg = new CustomLogicCfg(id, nodes, DefaultLogicType);
            logicCfg.Desc = desc;
            Add(id, logicCfg);
            return logicCfg;
        }

        public CustomLogicCfg GetCustomLogicCfg(int id)
        {
            if (TryGetValue(id, out var cfg))
                return cfg;
            return null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NoneParamBhvCfg Bhv<T>() where T : BehaviorNodeBase
        {
            return new NoneParamBhvCfg(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleNodeCfg Node<T>() where T : CustomNode
        {
            return new SimpleNodeCfg(typeof(T));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Log(string str)
        {
            return new DelegateBhvCfg(node => { CLHelper.LogInfo(node, str); });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg LogInt(string str, string intVar)
        {
            return new DelegateBhvCfg(node => { CLHelper.LogInfo(node, string.Format(str, node.GetVar<int>(intVar))); });
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg Log<VT>(string str, string var)
        {
            return new DelegateBhvCfg(node => { CLHelper.LogInfo(node, string.Format(str, node.GetVar<VT>(var))); });
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg Seq(List<ICustomNodeCfg> nodeCfgList)
        {
            return new SequenceBhvCfg(nodeCfgList, 1, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg Seq(params ICustomNodeCfg[] nodeCfgList)
        {
            return new SequenceBhvCfg(new List<ICustomNodeCfg>(nodeCfgList), 1, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg LoopSeq(List<ICustomNodeCfg> nodeCfgList, int loopCnt, float loopInterval)
        {
            return new SequenceBhvCfg(nodeCfgList, loopCnt, loopInterval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg LoopSeq(List<ICustomNodeCfg> nodeCfgList, string loopCntVar, float loopInterval)
        {
            return new SequenceBhvCfg(nodeCfgList).WithLoopCnt(loopCntVar).WithLoopInterval(loopInterval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg LoopSeq(List<ICustomNodeCfg> nodeCfgList, string loopCntVar, string loopIntervalVar)
        {
            return new SequenceBhvCfg(nodeCfgList).WithLoopCnt(loopCntVar).WithLoopInterval(loopIntervalVar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg LoopForever(List<ICustomNodeCfg> nodeCfgList, float loopInterval)
        {
            return new SequenceBhvCfg(nodeCfgList, -1, loopInterval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg LoopForever(List<ICustomNodeCfg> nodeCfgList, string loopInterval = null, float defaultInterval = 0f)
        {
            return new SequenceBhvCfg(nodeCfgList, new IntCfg(-1), new FloatCfg(loopInterval, defaultInterval));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceBhvCfg LoopForever(params ICustomNodeCfg[] nodeCfgList)
        {
            return new SequenceBhvCfg(new List<ICustomNodeCfg>(nodeCfgList), -1, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParallelBhvCfg Parallel(List<ICustomNodeCfg> nodeCfgList)
        {
            return new ParallelBhvCfg(nodeCfgList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParallelBhvCfg Parallel(params ICustomNodeCfg[] nodeCfgList)
        {
            return new ParallelBhvCfg(new List<ICustomNodeCfg>(nodeCfgList));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FTDelayBhvCfg Delay(string varID)
        {
            return new FTDelayBhvCfg(varID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FTDelayBhvCfg Delay(float timeLen)
        {
            return new FTDelayBhvCfg(timeLen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogicTempleteCfg Templete(int logicID)
        {
            return new LogicTempleteCfg(logicID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConditionBranchBhvCfg Branch(ICustomNodeCfg cndCfg, ICustomNodeCfg trueCfg = null, ICustomNodeCfg falseCfg = null)
        {
            return new ConditionBranchBhvCfg(cndCfg, trueCfg, falseCfg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomBhvStateCfg CustomState(string stateID, ICustomNodeCfg bhvCfg, ICustomNodeCfg exitBhvCfg = null, List<StateTransitionNodeCfg> transitions = null)
        {
            return new CustomBhvStateCfg() { StateID = stateID, Bhv = bhvCfg, ExitBhv = exitBhvCfg, Transitions = transitions };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomBhvStateCfg CustomState(string stateID, string nextStateID, ICustomNodeCfg bhvCfg, List<StateTransitionNodeCfg> transitions = null)
        {
            return new CustomBhvStateCfg() { StateID = stateID, NextStateID = nextStateID, Bhv = bhvCfg, Transitions = transitions };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomBhvStateCfg CustomState<T>(string stateID, ICustomNodeCfg bhvCfg, ICustomNodeCfg exitBhvCfg = null, List<StateTransitionNodeCfg> transitions = null) where T : CustomBhvState
        {
            return new CustomBhvStateCfg() { StateID = stateID, Bhv = bhvCfg, ExitBhv = exitBhvCfg, StateClass = typeof(T), Transitions = transitions };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CustomBhvStateCfg CustomState<T>(string stateID, string nextStateID, ICustomNodeCfg bhvCfg, List<StateTransitionNodeCfg> transitions = null) where T : CustomBhvState
        {
            return new CustomBhvStateCfg() { StateID = stateID, NextStateID = nextStateID, Bhv = bhvCfg, StateClass = typeof(T), Transitions = transitions };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StateNodeCfg State<T>(string stateID) where T : StateNode
        {
            return new StateNodeCfg() { StateID = stateID, StateClass = typeof(T) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FSMNodeCfg FSM(string defaultState, List<ICustomNodeCfg> stateList, List<StateTransitionNodeCfg> transitions = null)
        {
            return new FSMNodeCfg() { DefaultState = defaultState, StateList = stateList, GlobalTransitions = transitions };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FSMNodeCfg FSM<T>(string defaultState, List<ICustomNodeCfg> stateList, List<StateTransitionNodeCfg> transitions = null) where T : FSMNode
        {
            return new FSMNodeCfg() { DefaultState = defaultState, StateList = stateList, FsmImpType = typeof(T), GlobalTransitions = transitions };
        }

        public DelegateConditionCfg HasVar<T>(string varID)
        {
            return new DelegateConditionCfg((CustomNode node) => { return node.HasVar<T>(varID); });
        }

        public DelegateConditionCfg GetBool(string varID)
        {
            return new DelegateConditionCfg((CustomNode node) =>
            {
                if (!node.HasVar<bool>(varID))
                    return false;
                return node.GetVar<bool>(varID);
            });
        }

        public DelegateConditionCfg NotHasVar<T>(string varID)
        {
            return new DelegateConditionCfg((CustomNode node) => { return !node.HasVar<T>(varID); });
        }

        public DelegateConditionCfg IntVarEqual(string varID, int v)
        {
            return new DelegateConditionCfg((CustomNode node) => { return node.GetVar<int>(varID) == v; });
        }

        public DelegateConditionCfg IntVarGreater(string varID, int v)
        {
            return new DelegateConditionCfg((CustomNode node) =>
            {
                var getV = node.GetVar<int>(varID);
                return getV > v;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg SetVarOnBegin<T>(string varID, T value)
        {
            return new DelegateBhvCfg((CustomNode node) => { node.SetVar(varID, value); });
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg DestroyCall(NodeParamAction func)
        {
            return new DelegateBhvCfg(func)
            {
                DestroyCall = true,
                BeginCall = false,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg InitializeCall(NodeParamAction func)
        {
            return new DelegateBhvCfg(func)
            {
                InitializeCall = true,
                BeginCall = false,
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg SetVarOnInit<T>(string varID, T value)
        {
            return InitializeCall((CustomNode node) => { node.SetVar(varID, value); });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg ClearVarBhv<T>(string varID)
        {
            return new DelegateBhvCfg((CustomNode node) => { node.ClearVar<T>(varID); });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg LogVar<T>(string varID)
        {
            return new DelegateBhvCfg((CustomNode node) =>
            {
                var v = node.GetVar<T>(varID);
                CLHelper.LogInfo(node, $"LogVar varID={varID}, v={v}");
            });
        }
    }
}
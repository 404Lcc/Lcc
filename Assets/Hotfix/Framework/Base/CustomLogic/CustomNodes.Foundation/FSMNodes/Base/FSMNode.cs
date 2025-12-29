using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        private static bool _FSMNodeCfg = Register(typeof(FSMNodeCfg), NodeCategory.FSM);
    }

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
    public class FSMNodeCfg : IFSMNodeCfg, ICustomNodeCfg, IParseFromXml
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

        public FSMNodeCfg()
        {
        }

        public virtual bool ParseFromXml(XmlNode xmlNode)
        {
            var stateList = new NodeCfgList<ICustomNodeCfg>();
            StateList = stateList;
            if (!stateList.ParseFromXml(xmlNode, "State"))
            {
                return false;
            }

            if (StateList.Count == 0)
            {
                CLHelper.LogError(xmlNode, "FSMNodeCfg.ParseFromXml() StateList.Count == 0");
                CLHelper.AssertBreak();
                return false;
            }

            MaxTransitionInOneFrame = 1;
            string str = XmlHelper.GetAttribute(xmlNode, "MaxTransitionInOneFrame");
            if (!string.IsNullOrEmpty(str))
            {
                MaxTransitionInOneFrame = int.Parse(str);
                if (MaxTransitionInOneFrame <= 0)
                    MaxTransitionInOneFrame = 1;
            }

            var xmlTransitions = xmlNode.SelectSingleNode("GlobalTransitions");
            if (xmlTransitions != null)
            {
                var globalTransitions = new NodeCfgList<StateTransitionNodeCfg>();
                GlobalTransitions = globalTransitions;
                if (!globalTransitions.ParseFromXml(xmlTransitions))
                {
                    return false;
                }
            }

            DefaultState = XmlHelper.GetString(xmlNode, "DefaultState");
            RecordCurStateIDTo = XmlHelper.GetString(xmlNode, "RecordCurStateIDTo");
            return true;
        }
    }

    /// <summary>
    /// 运行时节点: 状态机
    /// </summary>
    public class FSMNode : CustomNode, INeedUpdate, INeedStopCheck
    {
        IFSMNodeCfg mCfg;
        protected Dictionary<string, StateNode> mStates = new();
        private string mDefaultStateID;
        private StateNode mCurrentState = null;

        protected List<StateTransitionNode> mTransitions = new();


        private HashSet<int> mUsedTempLogicSet = new();
        private Dictionary<string, ICustomNodeCfg> tempStateCfgs = new(16);

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);

            var theCfg = cfg as IFSMNodeCfg;
            mDefaultStateID = theCfg.DefaultState;
            mStates.Clear();

            if (theCfg.CachedStateCfgList == null)
            {
                mUsedTempLogicSet.Clear();
                tempStateCfgs.Clear();
                Inner_InitializeStates(theCfg, context, mUsedTempLogicSet);
                theCfg.CachedStateCfgList = new(tempStateCfgs.Count);
                foreach (var item in tempStateCfgs)
                {
                    theCfg.CachedStateCfgList.Add(item.Value);
                }
            }

            foreach (var stateCfg in theCfg.CachedStateCfgList)
            {
                StateNode stateNode = mContext.Factory.CreateCustomNode(stateCfg, context) as StateNode;
                stateNode.Deactivate();
                mStates.Add(stateNode.StateID, stateNode);
            }

            if (theCfg.GlobalTransitions != null)
            {
                for (int i = 0; i < theCfg.GlobalTransitions.Count; ++i)
                {
                    ICustomNodeCfg bhvCfg = theCfg.GlobalTransitions[i];
                    var transNode = mContext.Factory.CreateCustomNode(bhvCfg, context) as StateTransitionNode;
                    if (!CLHelper.Assert(transNode != null))
                        continue;
                    mTransitions.Add(transNode);
                }
            }

            mCfg = theCfg;
            mCurrentState = null;
            CLHelper.Assert(mStates.Count > 0);

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
            // var stateNode = mContext.NodeFactory.CreateCustomNode(stateNodeCfg, context) as StateNode;
            // if (stateNode == null)
            // {
            //     return;
            // }
            // stateNode.Deactivate();
            // var stateID = stateNode.StateID;
            // if (mStates.TryGetValue(stateID, out var exist_state_node))
            // {
            //     KaLog.LogInfo($"FSM 替换已有的State stateID={stateID}");
            //     mContext.NodeFactory.DestroyCustomNode(exist_state_node);
            //     mStates[stateID] = stateNode;
            // }
            // else
            // {
            //     KaLog.LogInfo($"FSM 添加State stateID={stateID}");
            //     mStates[stateID] = stateNode;
            // }

            var stateID = stateNodeCfg.StateID;
            // if (tempStateCfgs.TryGetValue(stateID, out var exist_state_node))
            // {
            //     KaLog.LogInfo($"FSM 替换已有的State stateID={stateID}");
            //     tempStateCfgs[stateID] = stateNodeCfg;
            // }
            // else
            // {
            //     KaLog.LogInfo($"FSM 添加State stateID={stateID}");
            //     tempStateCfgs.Add(stateID, stateNodeCfg);
            // }
            tempStateCfgs[stateID] = stateNodeCfg;
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            base.CollectInterfaceInChildren<T>(ref interfaceList);
            foreach (var item in mStates)
            {
                TraverseCollectInterface(ref interfaceList, item.Value);
            }

            foreach (var transNode in mTransitions)
            {
                TraverseCollectInterface(ref interfaceList, transNode);
            }
        }

        public override void Activate()
        {
            base.Activate();
            if (mCurrentState != null)
            {
                mCurrentState.Activate();
            }

            foreach (var transNode in mTransitions)
            {
                transNode.Activate();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            if (mCurrentState != null && mCurrentState.IsActive)
            {
                mCurrentState.Deactivate();
            }

            foreach (var transNode in mTransitions)
            {
                transNode.Deactivate();
            }
        }

        public override void Destroy()
        {
            if (mCurrentState != null)
            {
                mCurrentState.Exit();
            }

            foreach (var item in mStates)
            {
                mContext.Factory.DestroyCustomNode(item.Value);
            }

            mStates.Clear();

            foreach (var transNode in mTransitions)
            {
                mContext.Factory.DestroyCustomNode(transNode);
            }

            mTransitions.Clear();

            mCurrentState = null;
            mDefaultStateID = null;
            mCfg = null;

            base.Destroy();
        }


        public virtual bool CanStop()
        {
            if (mStates.Count == 0)
                return true;
            return false;
        }


        public virtual float Update(float dt)
        {
            if (mStates.Count == 0)
                return dt;

            if (mCurrentState == null)
            {
                TransToState(mDefaultStateID);
            }

            //先检查全局的Transitions
            foreach (var transNode in mTransitions)
            {
                var transGoalStateID = transNode.CheckTransitions();
                if (transGoalStateID != null)
                {
                    if (mCurrentState.StateID != transGoalStateID)
                    {
                        TransToState(transGoalStateID);
                    }
                }
            }

            if (mCurrentState == null)
            {
                return dt;
            }

            //支持一帧内连续切换 MaxTransitionInOneFrame 个状态
            for (int i = 0; i < mCfg.MaxTransitionInOneFrame; i++)
            {
                //检查状态转移
                var oldStateID = mCurrentState.StateID;
                var goalStateID = mCurrentState.CheckTransitions();
                if (goalStateID == oldStateID)
                {
                    break; //如果没有状态转移
                }

                var mGoalState = FindState(goalStateID);
                if (mGoalState != null)
                {
                    mCurrentState.Exit();
                    mCurrentState = mGoalState;
                    mCurrentState.Enter();
                }
            }

            return mCurrentState.Update(dt);
        }

        public StateNode CurrentState
        {
            get { return mCurrentState; }
        }

        public string CurrentStateID
        {
            get
            {
                if (mCurrentState == null)
                    return null;
                return mCurrentState.StateID;
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

            mCurrentState?.Exit();

            mCurrentState = goalState;
            mCurrentState.Enter();

            //记录当前状态到黑板
            if (!string.IsNullOrEmpty(mCfg.RecordCurStateIDTo))
            {
                SetVar(mCfg.RecordCurStateIDTo, goalStateID);
            }
        }


        private StateNode FindState(string stateID)
        {
            if (stateID == null)
            {
                return null;
            }

            if (mStates.TryGetValue(stateID, out var state))
            {
                return state;
            }

            return null;
        }

        public override void Reset()
        {
            foreach (var transNode in mTransitions)
            {
                transNode.Reset();
            }

            if (mCurrentState != null)
                mCurrentState.Exit();

            mCurrentState = FindState(mDefaultStateID);

            if (mCurrentState != null)
                mCurrentState.Enter();
        }
    }
}
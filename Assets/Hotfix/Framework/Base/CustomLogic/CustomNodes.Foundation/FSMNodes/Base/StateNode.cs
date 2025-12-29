using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public interface IStateNodeCfg
    {
        string StateID { get; }

        string NextStateID { get; }

        List<StateTransitionNodeCfg> Transitions { get; }

        System.Type StateClass { get; }
        System.Type NodeType();
    }

    /// <summary>
    /// 静态配置
    /// </summary>
    public class StateNodeCfg : IStateNodeCfg, ICustomNodeCfg, IParseFromXml
    {
        //命名的状态ID
        public string StateID { get; set; }

        //自动进入的下一个状态（可以不配）
        public string NextStateID { get; set; }

        //跳转逻辑（可以不配）
        public List<StateTransitionNodeCfg> Transitions { get; set; } = null;

        public System.Type StateClass { get; set; } = typeof(StateNode);

        public virtual System.Type NodeType()
        {
            return StateClass;
        }


        public StateNodeCfg()
        {
        }


        public virtual bool ParseFromXml(XmlNode xmlNode)
        {
            StateID = XmlHelper.GetAttribute(xmlNode, "StateID");
            if (!CLHelper.Assert(StateID != null, "StateNodeCfg StateID == null"))
            {
                return false;
            }

            NextStateID = XmlHelper.GetAttribute(xmlNode, "NextStateID");

            Transitions = XmlHelper.GetNodeList<StateTransitionNodeCfg>(xmlNode, "Transition");
            return true;
        }
    }

    /// <summary>
    /// 运行时节点: 基类状态
    /// </summary>
    public class StateNode : CustomNode, INeedUpdate
    {
        //缓存静态配置：当前状态ID，不可动态修改
        public string StateID { get; private set; }

        //缓存静态配置：缺省进入的下一个状态，这个值通常是固定值，不动态修改
        protected string DefaultNextStateID { get; private set; }

        protected List<StateTransitionNode> mTransitions = new();


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            IStateNodeCfg theCfg = cfg as IStateNodeCfg;

            StateID = theCfg.StateID;
            DefaultNextStateID = theCfg.NextStateID;

            if (theCfg.Transitions != null)
            {
                for (int i = 0; i < theCfg.Transitions.Count; ++i)
                {
                    ICustomNodeCfg bhvCfg = theCfg.Transitions[i];
                    var transNode = mContext.Factory.CreateCustomNode(bhvCfg, context) as StateTransitionNode;
                    if (!CLHelper.Assert(transNode != null))
                        continue;
                    mTransitions.Add(transNode);
                }
            }

            if (string.IsNullOrEmpty(StateID))
            {
                this.LogError($"StateNode string.IsNullOrEmpty(mStateID)");
            }
        }

        public override void Destroy()
        {
            StateID = null;
            DefaultNextStateID = null;
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                mContext.Factory.DestroyCustomNode(mTransitions[i]);
            }

            mTransitions.Clear();
            base.Destroy();
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            base.CollectInterfaceInChildren<T>(ref interfaceList);

            if (mTransitions == null)
                return;
            for (int i = 0; i < mTransitions.Count; ++i)
            {
                TraverseCollectInterface(ref interfaceList, mTransitions[i]);
            }
        }

        public virtual float Update(float dt)
        {
            foreach (var transNode in mTransitions)
            {
                transNode.Update(dt);
            }

            return dt;
        }


        public virtual void Enter()
        {
            this.Activate();
            foreach (var transNode in mTransitions)
            {
                transNode.Reset();
                transNode.Activate();
            }
        }

        public virtual void Exit()
        {
            this.Deactivate();
            foreach (var transNode in mTransitions)
            {
                transNode.Deactivate();
            }
        }

        public virtual string CheckTransitions()
        {
            foreach (var transNode in mTransitions)
            {
                var goal_state = transNode.CheckTransitions();
                if (goal_state != null)
                {
                    return goal_state;
                }
            }

            return DefaultNextStateID;
        }
    }
}
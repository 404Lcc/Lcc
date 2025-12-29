using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        private static bool _ConditionBranchBhvCfg = Register(typeof(ConditionBranchBhvCfg), NodeCategory.Bhv);
    }

    /// <summary>
    /// 静态配置
    /// </summary>
    public class ConditionBranchBhvCfg : ICustomNodeCfg, IParseFromXml
    {
        //判断条件配置
        public ICustomNodeCfg CndCfg { get; protected set; }

        //条件达成行为配置
        public ICustomNodeCfg TrueBhvCfg { get; protected set; }

        //条件不达成行为配置
        public ICustomNodeCfg FalseBhvCfg { get; protected set; }
        public bool CheckOnTick { get; protected set; } = true;

        public System.Type NodeType()
        {
            return typeof(ConditionBranchBhv);
        }

        public ConditionBranchBhvCfg()
        {
        }

        public ConditionBranchBhvCfg(ICustomNodeCfg cndCfg, ICustomNodeCfg trueCfg = null, ICustomNodeCfg falseCfg = null)
        {
            CndCfg = cndCfg;
            TrueBhvCfg = trueCfg;
            FalseBhvCfg = falseCfg;
        }

        public bool ParseFromXml(XmlNode xmlNode)
        {
            CndCfg = CLHelper.CreateNodeCfg(xmlNode.SelectSingleNode("Condition"));
            TrueBhvCfg = CLHelper.CreateNodeCfg(xmlNode.SelectSingleNode("TrueBhv"));
            FalseBhvCfg = CLHelper.CreateNodeCfg(xmlNode.SelectSingleNode("FalseBhv"));

            if (!CLHelper.Assert(TrueBhvCfg != null || FalseBhvCfg != null))
            {
                return false;
            }

            var categoryCnd = NodeConfigTypeRegistry.GetNodeCfgCategory(CndCfg.GetType());
            CLHelper.Assert(categoryCnd == NodeCategory.Cnd);

            if (TrueBhvCfg != null)
            {
                var categoryBhv1 = NodeConfigTypeRegistry.GetNodeCfgCategory(TrueBhvCfg.GetType());
                CLHelper.Assert(categoryBhv1 == NodeCategory.Bhv);
            }

            if (FalseBhvCfg != null)
            {
                var categoryBhv2 = NodeConfigTypeRegistry.GetNodeCfgCategory(FalseBhvCfg.GetType());
                CLHelper.Assert(categoryBhv2 == NodeCategory.Bhv);
            }

            CheckOnTick = XmlHelper.GetBool(xmlNode, "CheckOnTick");

            return true;
        }
    }

    /// <summary>
    /// 按条件触发的行为节点：条件 + 行为
    /// </summary>
    public class ConditionBranchBhv : BehaviorNodeBase, INeedStopCheck
    {
        protected ConditionNodeBase mCondition = null; //激活条件
        protected BehaviorNodeBase mTrueBhv = null; //附带行为
        protected BehaviorNodeBase mFalseBhv = null; //附带行为

        protected bool? mIsConditionReached = null;
        protected bool mCheckOnTick = false;


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            ConditionBranchBhvCfg theCfg = cfg as ConditionBranchBhvCfg;

            mCondition = mContext.Factory.CreateCustomNode(theCfg.CndCfg, context) as ConditionNodeBase;

            //行为一开始处于非激活状态
            if (theCfg.TrueBhvCfg != null)
            {
                mTrueBhv = mContext.Factory.CreateCustomNode(theCfg.TrueBhvCfg, context) as BehaviorNodeBase;
                mTrueBhv.Deactivate();
            }

            if (theCfg.FalseBhvCfg != null)
            {
                mFalseBhv = mContext.Factory.CreateCustomNode(theCfg.FalseBhvCfg, context) as BehaviorNodeBase;
                mFalseBhv.Deactivate();
            }

            CLHelper.Assert(mCondition != null);
            CLHelper.Assert(mTrueBhv != null || mFalseBhv != null);

            mIsConditionReached = null;
            mCheckOnTick = theCfg.CheckOnTick;
            if (mCondition is INeedUpdate)
            {
                mCheckOnTick = true;
            }
        }

        public override void Destroy()
        {
            mContext.Factory.DestroyCustomNode(mCondition);
            mCondition = null;

            mContext.Factory.DestroyCustomNode(mTrueBhv);
            mTrueBhv = null;

            mContext.Factory.DestroyCustomNode(mFalseBhv);
            mFalseBhv = null;

            mIsConditionReached = null;
            mCheckOnTick = false;
            base.Destroy();
        }

        public override void Reset()
        {
            base.Reset();
            mIsConditionReached = null;
            mCondition?.Reset();
            mTrueBhv?.Reset();
            mFalseBhv?.Reset();
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            TraverseCollectInterface<T>(ref interfaceList, mCondition);
            if (mTrueBhv != null)
            {
                TraverseCollectInterface<T>(ref interfaceList, mTrueBhv);
            }

            if (mFalseBhv != null)
            {
                TraverseCollectInterface<T>(ref interfaceList, mFalseBhv);
            }
        }

        public override void Activate()
        {
            base.Activate();
            mCondition?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            mCondition?.Deactivate();
            mTrueBhv?.Deactivate();
            mFalseBhv?.Deactivate();
        }

        public bool CanStop()
        {
            // 1. 条件检查是否能被停止
            INeedStopCheck cndNF = mCondition as INeedStopCheck;
            if (cndNF != null && !cndNF.CanStop())
            {
                return false;
            }

            // 2. 条件达成后，行为是否能被停止
            var bhv = mFalseBhv;
            if (mIsConditionReached == true)
            {
                bhv = mTrueBhv;
            }

            if (bhv != null && bhv is INeedStopCheck bhvNSC)
            {
                return bhvNSC.CanStop();
            }

            return true;
        }

        protected override void OnBegin()
        {
            Inner_CheckConditionReached();
        }

        protected override float OnUpdate(float dt)
        {
            if (mCondition is INeedUpdate updateCnd)
            {
                updateCnd.Update(dt);
            }

            if (mCheckOnTick)
            {
                Inner_CheckConditionReached();
            }

            if (mIsConditionReached == true)
            {
                if (mTrueBhv != null)
                {
                    dt = mTrueBhv.Update(dt);
                }
            }
            else
            {
                if (mFalseBhv != null)
                {
                    dt = mFalseBhv.Update(dt);
                }
            }

            return dt;
        }

        protected void Inner_CheckConditionReached()
        {
            var isReached = mCondition.IsConditionReached();
            bool hasChange = mIsConditionReached != isReached;
            mIsConditionReached = isReached;
            if (!hasChange)
            {
                return;
            }

            if (mIsConditionReached == true)
            {
                mTrueBhv?.Activate();
                mFalseBhv?.Deactivate();
            }
            else
            {
                mTrueBhv?.Deactivate();
                mFalseBhv?.Activate();
            }
        }
    }
}
/********************************************************************
*  CustomLogic 自定义逻辑，是由一系列Node子逻辑的自由组合
*  CustomLogic 本身是一个逻辑根节点，拥有其他节点相同的性质。
    特殊职责在于：
    作为外部输入的门面（如：update、接口方式的事件通知）
    存放一块公用的数据黑板
*  可以在CustomLogic的特化子类（如SkillLogic、BuffLogic），扩展对外部输入响应的种类。
*  出于各种权衡，外部输入的方式 主要采用显视接口调用的形式，而不是Event、Signal
*********************************************************************/

using System;
using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        private static bool _CustomLogicConfig = Register(typeof(CustomLogicCfg), NodeCategory.Mixture);
    }

    public interface ICustomLogicCfg : IHasSubNodeCfgList
    {
        public int ID { get; }
        public System.Type LogicType { get; }
        public VarEnv DefaultVarEnv { get; }
    }

    /// <summary>
    /// 自定义逻辑静态配置，装配信息
    /// </summary>
    public class CustomLogicCfg : ICustomNodeCfg, ICustomLogicCfg, IParseFromXml
    {
        private List<ICustomNodeCfg> _nodeCfgList = new();

        //运行时类型
        private System.Type _logicType = typeof(CustomLogic);

        /// <summary>
        /// 逻辑配置ID
        /// </summary>
        public int ID { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        public System.Type LogicType
        {
            get { return _logicType; }
            set
            {
                if (value == _logicType)
                    return;

                if (value == null || !value.IsSubclassOf(typeof(CustomLogic)))
                {
                    LogWrapper.LogError($"校验：CustomLogicCfg.LogicType set 必须继承自 CustomLogic, v={value}");
                    return;
                }

                _logicType = value;
            }
        }

        //只在前置黑板没有值的时候提供缺省值， 如果发现前置黑板已经有值就会跳过
        public VarEnv DefaultVarEnv { get; protected set; } = null;

        public virtual System.Type NodeType()
        {
            return LogicType;
        }

        public List<ICustomNodeCfg> GetNodeCfgList()
        {
            return _nodeCfgList;
        }


        public void DefaultVar(Action<VarEnv> initVarEnv)
        {
            DefaultVarEnv ??= new VarEnv(); //静态配置不走对象池，直接 new
            initVarEnv(DefaultVarEnv);
        }


        public CustomLogicCfg()
        {
        }

        public CustomLogicCfg(int id, List<ICustomNodeCfg> nodeCfgList, System.Type logicType = null, string desc = null)
        {
            ID = id;
            _nodeCfgList = nodeCfgList;
            if (logicType != null)
            {
                LogicType = logicType;
            }

            Desc = desc;
        }


        public virtual bool ParseFromXml(System.Xml.XmlNode cfgNode)
        {
            int id = cfgNode.GetSingleNodeID();

            //逐个解析子节点
            XmlNodeList customNodeList = cfgNode.SelectNodes("Node");
            if (customNodeList == null)
                return false;

            this.ID = id;
            this._nodeCfgList.Clear();
            foreach (XmlNode customNode in customNodeList)
            {
                try
                {
                    ICustomNodeCfg nodeCfg = CLHelper.CreateNodeCfg(customNode);
                    this._nodeCfgList.Add(nodeCfg);
                }
                catch (System.Exception e)
                {
                    CLHelper.LogError(customNode, "FillCustomLogicCfg Failed, config ID [" + this.ID + "]");
                    //KLogger.LogError(e);
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 自定义逻辑运行时： 逻辑根节点，由多个子节点构成
    /// </summary>
    public class CustomLogic : CustomNode, INeedUpdate, INeedStopCheck
    {
        private HashSet<int> mUsedTempLogicSet = new();

        //CustomLogic系统外界信息的通知, 将通知传播给各个CustomNode和其子Node
        private List<INeedUpdate> mNeedUpdateList = new List<INeedUpdate>();
        private List<INeedStopCheck> mNeedStopCheckList = new List<INeedStopCheck>();

        //自定义的子节点
        protected List<ICustomNode> mNodes;

        public CustomLogic()
        {
            mNodes = new List<ICustomNode>();
        }

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);

            var logicCfg = cfg as CustomLogicCfg;

            mUsedTempLogicSet.Clear();
            Inner_InitializeNodes(logicCfg, context, mUsedTempLogicSet);
        }

        private void Inner_InitializeNodes(ICustomLogicCfg logicCfg, CustomNodeContext context, HashSet<int> usedTempLogicSet)
        {
            if (logicCfg == null)
            {
                this.LogError($"_InitializeNodes logicCfg == null");
                return;
            }

            logicCfg.DefaultVarEnv?.CopyTo(context.VarEnvImp);

            var nodeCfgList = logicCfg.GetNodeCfgList();
            //装配节点
            for (int i = 0; i < nodeCfgList.Count; i++)
            {
                var nodeCfg = nodeCfgList[i];
                //////////////////////////// 处理模板引用 Begin ////////////////////////////
                var templeteCfg = nodeCfg as LogicTempleteCfg;
                if (templeteCfg != null)
                {
                    int templeteID = templeteCfg.LogicID;
                    if (usedTempLogicSet.Contains(templeteID))
                    {
                        this.LogError($"ERROR: 循环引用CustomLogic模板! RootLogicID={logicCfg.ID}, templeteID={templeteID}");
                        continue;
                    }

                    usedTempLogicSet.Add(templeteID);
                    var cfgContainer = context.ConfigContainer;
                    if (cfgContainer == null)
                    {
                        this.LogError($"ERROR: CustomLogic使用模板, 但找不到设定模板库！cfgContainer != null, RootLogicID={logicCfg.ID}, templeteID={templeteID}");
                        continue;
                    }

                    var templeteLogicCfg = cfgContainer.GetCustomLogicCfg(templeteID);
                    if (templeteLogicCfg != null)
                    {
                        //插入模板CustomLogic所配置的各个节点
                        Inner_InitializeNodes(templeteLogicCfg, context, usedTempLogicSet);
                    }
                    else
                    {
                        this.LogError($"ERROR: CustomLogic模板找不到! RootLogicID={logicCfg.ID}, templeteID={templeteID}");
                    }

                    continue;
                }
                ////////////////////////////// 处理模板引用 End ////////////////////////////

                CustomNode theNode = mContext.Factory.CreateCustomNode(nodeCfg, context);
                this.AddCustomNode(theNode);
            }
        }

        public override void Destroy()
        {
            mUsedTempLogicSet.Clear();
            var factory = mContext.Factory;
            //节点
            for (int i = 0; i < mNodes.Count; ++i)
            {
                factory.DestroyCustomNode(mNodes[i]);
            }

            mNodes.Clear();
            ClearInterfaceCache();

            //黑板
            factory.DestroyPart(VarEnvRef);
            factory.DestroyPart(GenInfo);

            base.Destroy();
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            for (int i = 0; i < mNodes.Count; ++i)
            {
                TraverseCollectInterface(ref interfaceList, mNodes[i]);
            }
        }

        internal virtual void AddCustomNode(CustomNode node)
        {
            mNodes.Add(node);
            CacheInterface(node);
        }

        protected virtual void ClearInterfaceCache()
        {
            mNeedUpdateList.Clear();
            mNeedStopCheckList.Clear();
        }

        protected virtual void CacheInterface(CustomNode node)
        {
            //Update作为Node默认的标准驱动方式具有一定特殊性， 父节点去管理子节点的Update
            if (node is INeedUpdate updateNode)
            {
                mNeedUpdateList.Add(updateNode);
            }

            TraverseCollectInterface(ref mNeedStopCheckList, node);
        }

        public virtual float Update(float dt)
        {
            for (int i = 0; i < mNeedUpdateList.Count; ++i)
            {
                var iupdate = mNeedUpdateList[i];
                var node = iupdate as ICustomNode;
                if (node != null && node.IsActive)
                {
                    iupdate.Update(dt);
                }
            }

            return dt;
        }

        //CustomLogic逻辑的生存周期： 默认是刚创建就可以被销毁
        //除非某些Node通过NeedStopCheck, 表达自己当前不能被销毁，如果被打断可能会出问题
        public virtual bool CanStop()
        {
            for (int i = 0; i < mNeedStopCheckList.Count; ++i)
            {
                var stopcheck = mNeedStopCheckList[i];
                var node = stopcheck as ICustomNode;
                if (node != null && node.IsActive)
                {
                    if (!stopcheck.CanStop())
                        return false;
                }
            }

            return true;
        }
    }
}
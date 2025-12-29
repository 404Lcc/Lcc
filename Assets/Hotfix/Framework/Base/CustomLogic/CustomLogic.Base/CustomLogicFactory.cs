/********************************************************************
 *  CustomLogicFactory 自定义逻辑工厂
 *　负责根据使用方初始数据、配置信息，构造各个条件、行为，装配出一个CustomLogic。
 *　每个CustomLogic配置有一个ID做标识
 *********************************************************************/

using System.Collections.Generic;

namespace LccHotfix
{
    public class CustomLogicFactory
    {
        protected Dictionary<string, ILogicConfigContainer> mConfigContainerDic = new();

        //缓存
        protected CLNodesPool<ICustomNode> _nodesPool = new();
        protected CLNodesPool<ICanRecycle> _partsPool = new();
        public CLNodesPool<ICustomNode> NodePool => _nodesPool;
        public CLNodesPool<ICanRecycle> PartsPool => _partsPool;


        public virtual ILogicConfigContainer AddConfigContainer(ILogicConfigContainer cfgContainer)
        {
            mConfigContainerDic.Add(cfgContainer.ContainerName, cfgContainer);
            return cfgContainer;
        }
        public Dictionary<string, ILogicConfigContainer> ConfigContainer
        {
            get { return mConfigContainerDic; }
        }

        public virtual void DoCache()
        {
        }

        //主方法：创建并装配一个自定义逻辑
        public CustomLogic CreateLogic(ICustomLogicGenInfo genInfo)
        {
            var cfgContainerName = genInfo.ConfigContainerName;
            if (!mConfigContainerDic.TryGetValue(cfgContainerName, out var cfgContainer))
            {
                CLHelper.Assert(false, $"CreateCustomLogic ConfigContainer = null, cfgContainerName={cfgContainerName}");
                return null;
            }

            var config = cfgContainer.GetCustomLogicCfg(genInfo.LogicConfigID);
            if (config == null)
            {
                CLHelper.Assert(false, $"CreateCustomLogic Cant Find Config : ConfigID={genInfo.LogicConfigID}, cfgContainerName={cfgContainerName}");
                return null;
            }

            CustomLogic customLogic = CreateLogic(genInfo, config, cfgContainer);
            return customLogic;
        }

        public CustomLogic CreateLogic(ICustomLogicGenInfo genInfo, CustomLogicCfg config, ILogicConfigContainer cfgContainer = null)
        {
            if (config == null)
            {
                CLHelper.Assert(false, "CreateCustomLogic config == null");
                return null;
            }

            System.Type logicType = config.NodeType();
            CustomLogic customLogic = _nodesPool.Create<CustomLogic>(logicType);

            if (genInfo.LogicConfigID != config.ID)
            {
                LogWrapper.LogError($"CreateLogic: genInfo.LogicConfigID({genInfo.LogicConfigID}) != config.ID({config.ID})");
                genInfo.LogicConfigID = config.ID;
            }

            //区别于CreateCustomNode
            VarEnv varEnv = genInfo.PreEnv ?? CreatePart<VarEnv>();
            CustomNodeContext context = new CustomNodeContext(genInfo, customLogic, varEnv, cfgContainer, this);
            customLogic.InitializeNode(config, context);

            return customLogic;
        }

        public CustomNode CreateCustomNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            //初始化行为
            System.Type nodeType = cfg.NodeType();
            var theNode = NodePool.Create<CustomNode>(nodeType);
            theNode.InitializeNode(cfg, context);
            return theNode;
        }

        public void DestroyCustomNode(ICustomNode node)
        {
            if (node != null)
            {
                _nodesPool.Destroy(node);
            }
        }

        public T CreatePart<T>() where T : class, ICanRecycle, new()
        {
            var obj = PartsPool.Create<T>();
            return obj;
        }

        public void DestroyPart(ICanRecycle obj)
        {
            if (obj != null)
            {
                PartsPool.Destroy(obj);
            }
        }
    }
}
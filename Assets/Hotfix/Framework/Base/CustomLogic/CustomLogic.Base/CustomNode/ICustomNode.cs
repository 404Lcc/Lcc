namespace LccHotfix
{
    //配置接口
    public interface ICustomNodeCfg
    {
        /// <summary>
        /// 节点静态配置对应的运行时节点类
        /// </summary>
        /// <returns></returns>
        System.Type NodeType();
    }

    //自定义节点
    public interface ICustomNode : ICanRecycle, IInterfaceCollector, ICanReset
    {
        bool IsActive { get; }

        void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context);

        void Activate();

        void Deactivate();
    }
}
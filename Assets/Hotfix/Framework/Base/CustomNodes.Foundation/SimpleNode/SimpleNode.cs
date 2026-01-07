namespace LccHotfix
{
    public class SimpleNodeCfg : ICustomNodeCfg
    {
        public System.Type RuntimeType { get; protected set; }

        public SimpleNodeCfg(System.Type bhvType)
        {
            RuntimeType = bhvType;
        }

        public System.Type NodeType()
        {
            return RuntimeType;
        }
    }
}
using System.Collections.Generic;

namespace LccHotfix
{
    //分类没有特别重要的结构意义，多用于类型检查
    public enum NodeCategory
    {
        Unknown, //未分类
        Bhv, //行为
        Cnd, //条件
        Event, //消息处理
        FSM, //状态机
        State, //状态
        StateTransition, //状态转移
        Mixture, //混合容器
    }

    public static partial class NodeConfigTypeRegistry
    {
        private static Dictionary<string, System.Type> m_node_name2type;
        private static Dictionary<System.Type, NodeCategory> m_node_category;

        private static void InitDic() //static成员的初始化顺序是不定的，加这个是为了保证Dic不为null
        {
            if (m_node_name2type == null)
                m_node_name2type = new Dictionary<string, System.Type>();
            if (m_node_category == null)
                m_node_category = new Dictionary<System.Type, NodeCategory>();
        }

        public static bool Register(System.Type type, NodeCategory nodeCategory = NodeCategory.Unknown)
        {
            InitDic();

#if UNITY_EDITOR
            System.Type existed_type;
            if (m_node_name2type.TryGetValue(type.Name, out existed_type))
            {
                if (type.Name != existed_type.Name)
                    LogWrapper.LogError($"NodeConfigTypeRegistry, {type.Name} has existed, FullName={type.FullName}");
            }
#endif

            m_node_name2type[type.Name] = type;
            m_node_category[type] = nodeCategory;

            return true;
        }

        public static ICustomNodeCfg CreateCustomNodeCfg(string typeName)
        {
            System.Type type = null;
            if (!m_node_name2type.TryGetValue(typeName, out type))
                return null;
            return System.Activator.CreateInstance(type) as ICustomNodeCfg;
        }

        public static NodeCategory GetNodeCfgCategory(System.Type nodeCfgType)
        {
            NodeCategory category = NodeCategory.Unknown;
            m_node_category.TryGetValue(nodeCfgType, out category);
            return category;
        }
    }
}
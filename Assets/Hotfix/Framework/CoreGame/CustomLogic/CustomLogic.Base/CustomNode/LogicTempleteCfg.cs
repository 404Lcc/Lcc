namespace LccHotfix
{
    /// <summary>
    /// CustomLogic模板节点， 等价于在该LogicTempletNode处直接插入TempletLogic的全部节点
    /// </summary>
    public class LogicTempleteCfg : ICustomNodeCfg
    {
        public int LogicID { get; protected set; }

        public System.Type NodeType()
        {
            LogWrapper.LogError("ERROR : try to Initialize LogicTempleteNode!");
            return null;
        }

        public LogicTempleteCfg(int logicID)
        {
            LogicID = logicID;
        }
    }
}
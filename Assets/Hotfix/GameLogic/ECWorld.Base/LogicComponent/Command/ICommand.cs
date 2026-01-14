namespace LccHotfix
{
    public interface IEntityCommandHandler
    {
        bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd);
    }

    public static partial class EntityCmdType
    {
        public const int Op_None = 0;
    }

    //值类型Cmd，自定义扩展用ParamEx
    public partial struct EntityCommand
    {
        public int CmdType { get; set; }
        public long EntityID { get; set; }
    }
}
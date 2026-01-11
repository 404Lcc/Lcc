namespace LccHotfix
{
    public interface IEntityCommandPreHandler
    {
        bool PreHandleCommand(LogicEntity owner, EntityCommand cmd);
        
    }

}
namespace LccHotfix
{
    public interface IEntityCommandHandler
    {
        bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd);
    }
    
    public interface IEntityCommandDispatcher : IEntityCommandHandler
    {
        void BindOwner(LogicEntity owner);
        void UnBindOwner();
    }
}
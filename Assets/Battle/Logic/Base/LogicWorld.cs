using Entitas;
using System;

public class LogicWorld : Context<LogicEntity>
{
    private readonly static int _totalComponents = LogicComponentsLookup.TotalComponents;
    private readonly static int _startCreationIndex = 0;
    private readonly static ContextInfo _contextInfo = new ContextInfo("Logic", LogicComponentsLookup.componentNames, LogicComponentsLookup.componentTypes);
    private readonly static Func<Entity, IAERC> _aercFactory = (entity) => new SafeAERC(entity);
    private readonly static Func<LogicEntity> _entityFactory = () => new LogicEntity();

    public LogicWorld() : base(_totalComponents, _startCreationIndex, _contextInfo, _aercFactory, _entityFactory)
    {
        OnEntityCreated += EntityCreated;
        OnEntityDestroyed += EntityDestroyed;
    }

    private void EntityCreated(IContext context, Entity iEntity)
    {
        var entity = (LogicEntity)iEntity;
        entity.Enter();
    }

    private void EntityDestroyed(IContext context, Entity iEntity)
    {
        var entity = (LogicEntity)iEntity;
        entity.Leave();
    }

    public LogicEntity GetEntityWithID(long id)
    {
        return ((PrimaryEntityIndex<LogicEntity, long>)GetEntityIndex(Worlds.ID)).GetEntity(id);
    }
}
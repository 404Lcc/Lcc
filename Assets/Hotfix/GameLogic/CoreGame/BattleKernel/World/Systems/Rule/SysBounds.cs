using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{

public class SysBounds : IExecuteSystem
{
    private IGroup<LogicEntity> _group;

    private readonly List<LogicEntity> _entityBuffer = new(256);
    private int _entityBufferVersion = -1;

    public SysBounds(ECWorlds world)
    {
        _group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComTransform, LogicComponentsLookup.ComBounds));
    }

    public void Execute()
    {
        var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
        foreach (var entity in buffer)
        {
            var comBounds = entity.comBounds;
            var comTransform = entity.comTransform;
            comBounds.UpdateBounds(comTransform.position);
        }
    }
}
}

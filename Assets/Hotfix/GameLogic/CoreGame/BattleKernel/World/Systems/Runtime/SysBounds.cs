using Entitas;

namespace LccHotfix
{

public class SysBounds : IExecuteSystem
{
    private IGroup<LogicEntity> _group;

    public SysBounds(ECWorlds world)
    {
        _group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComTransform, LogicComponentsLookup.ComBounds));
    }

    public void Execute()
    {
        foreach (var entity in _group.GetEntities())
        {
            var comBounds = entity.comBounds;
            var comTransform = entity.comTransform;
            comBounds.UpdateBounds(comTransform.position);
        }
    }
}
}

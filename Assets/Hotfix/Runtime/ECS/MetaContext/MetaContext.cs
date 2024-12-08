using Entitas;
using System;

namespace LccHotfix
{
    public class MetaContext : Context<MetaEntity>
    {
        private readonly static int _totalComponents = MetaComponentsLookup.TotalComponents;
        private readonly static int _startCreationIndex = 0;
        private readonly static ContextInfo _contextInfo = new ContextInfo("Meta", MetaComponentsLookup.componentNames.ToArray(), MetaComponentsLookup.componentTypes.ToArray());
        private readonly static Func<Entity, IAERC> _aercFactory = (entity) => new SafeAERC(entity);
        private readonly static Func<MetaEntity> _entityFactory = () => new MetaEntity();

        public MetaContext() : base(_totalComponents, _startCreationIndex, _contextInfo, _aercFactory, _entityFactory)
        {
            OnEntityCreated += EntityCreated;
            OnEntityDestroyed += EntityDestroyed;
        }
        private void EntityCreated(IContext context, Entity entity)
        {
            ((MetaEntity)entity).Enter(this);
        }
        private void EntityDestroyed(IContext context, Entity entity)
        {
            ((MetaEntity)entity).Leave();
        }
    }
}
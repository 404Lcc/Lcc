using Entitas;
using System;

namespace LccHotfix
{
    public partial class MetaContext : Context<MetaEntity>
    {
        protected MetaEntity _uniqueEntity;

        public MetaEntity UniqueEntity
        {
            get
            {
                if (_uniqueEntity == null)
                {
                    _uniqueEntity = CreateEntity();
                }

                return _uniqueEntity;
            }
        }

        public MetaContext(int totalComponents, int startCreationIndex, ContextInfo contextInfo, Func<Entity, IAERC> aercFactory, Func<MetaEntity> entityFactory) : base(totalComponents, startCreationIndex, contextInfo, aercFactory, entityFactory)
        {
            _uniqueEntity = CreateEntity();

            OnEntityCreated += EntityCreated;
            OnEntityDestroyed += EntityDestroyed;
            OnEntityWillBeDestroyed += EntityWillBeDestroyed;
        }

        private void EntityCreated(IContext context, Entity entity)
        {
            ((MetaEntity)entity).Enter(this);
        }

        private void EntityDestroyed(IContext context, Entity entity)
        {
            ((MetaEntity)entity).Leave();
        }

        private void EntityWillBeDestroyed(IContext context, Entity entity)
        {
            ((MetaEntity)entity).WillBeLeave();
        }

        public bool HasUniqueComponent(int index)
        {
            if (index < 0 || index >= MetaComponentsLookup.TotalComponents)
            {
                return false;
            }

            return _uniqueEntity.HasComponent(index);
        }

        public void SetUniqueComponent<T>(int index, T component) where T : MetaComponent
        {
            if (index < 0 || index >= MetaComponentsLookup.TotalComponents)
            {
                return;
            }

            var regType = MetaComponentsLookup.typeIndexList[index].componentType;
            if (regType != typeof(T))
            {
                return;
            }

            if (_uniqueEntity.HasComponent(index))
            {
                _uniqueEntity.ReplaceComponent(index, component);
            }
            else
            {
                _uniqueEntity.AddComponent(index, component);
            }
        }

        public T GetUniqueComponent<T>(int index) where T : MetaComponent
        {
            if (!HasUniqueComponent(index))
            {
                return null;
            }

            return (T)_uniqueEntity.GetComponent(index);
        }
    }
}
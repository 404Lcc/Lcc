using Entitas;
using System;

namespace LccHotfix
{
    public partial class MetaWorld : Context<MetaEntity>
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

        public MetaWorld(ContextInfo contextInfo, int totalComponents, Func<MetaEntity> entityFactory, int startCreationIndex = 0, Func<IEntity, IAERC> aercFactory = null) : base(totalComponents, startCreationIndex, contextInfo, aercFactory, entityFactory)
        {
            OnEntityCreated += EntityCreated;
            OnEntityDestroyed += EntityDestroyed;
            OnEntityWillBeDestroyed += EntityWillBeDestroyed;
        }

        private void EntityCreated(IContext context, IEntity entity)
        {
            ((MetaEntity)entity).Enter(this);
        }

        private void EntityDestroyed(IContext context, IEntity entity)
        {
            ((MetaEntity)entity).Leave();
        }

        private void EntityWillBeDestroyed(IContext context, IEntity entity)
        {
            ((MetaEntity)entity).WillBeLeave();
        }

        public bool HasUniqueComponent(int index)
        {
            if (index < 0 || index >= MetaComponentsLookup.TotalComponents)
            {
                return false;
            }

            return UniqueEntity.HasComponent(index);
        }

        public void SetUniqueComponent<T>(int index, T component) where T : MetaComponent
        {
            if (index < 0 || index >= MetaComponentsLookup.TotalComponents)
            {
                return;
            }

            var regType = MetaComponentsLookup.TypeIndexList[index].ComponentType;
            if (regType != typeof(T))
            {
                return;
            }

            if (UniqueEntity.HasComponent(index))
            {
                UniqueEntity.ReplaceComponent(index, component);
            }
            else
            {
                UniqueEntity.AddComponent(index, component);
            }
        }

        public T GetUniqueComponent<T>(int index) where T : MetaComponent
        {
            if (!HasUniqueComponent(index))
            {
                return null;
            }

            return (T)UniqueEntity.GetComponent(index);
        }
    }
}
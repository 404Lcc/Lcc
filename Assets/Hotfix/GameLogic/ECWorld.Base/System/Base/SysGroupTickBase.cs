using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public abstract class SysGroupTickBase : IExecuteSystem, ITearDownSystem, IInitializeSystem, ILateUpdateSystem
    {
        protected readonly LogicWorld _logicWorld;
        protected IGroup<LogicEntity> _group;

        //Group.GetEntities 2种接口都会 new一个临时数组，这里针对GC优化一下
        protected List<LogicEntity> _entitiesCache;
        protected bool _isEntitiesCacheDirty;

        public SysGroupTickBase(LogicWorld logicWorld)
        {
            _logicWorld = logicWorld;
            _entitiesCache = new(32);
            _isEntitiesCacheDirty = true;
        }

        public void Initialize()
        {
            _group = InnerGetGroup();
            _group.OnEntityAdded += OnEntityAdded;
            _group.OnEntityRemoved += OnEntityRemoved;
        }

        public void TearDown()
        {
            _group.OnEntityAdded -= OnEntityAdded;
            _group.OnEntityRemoved -= OnEntityRemoved;
        }

        protected void OnEntityAdded(IGroup<LogicEntity> group, LogicEntity entity, int index, IComponent component)
        {
            _isEntitiesCacheDirty = true;
        }

        protected void OnEntityRemoved(IGroup<LogicEntity> group, LogicEntity entity, int index, IComponent component)
        {
            _isEntitiesCacheDirty = true;
        }

        public void Execute()
        {
            float dt = Time.deltaTime;

            if (_isEntitiesCacheDirty)
            {
                _entitiesCache.Clear();
                foreach (var entity in _group)
                {
                    _entitiesCache.Add(entity);
                }

                _isEntitiesCacheDirty = false;
            }

            UpdateEntities(_entitiesCache, dt);
        }


        protected abstract IGroup<LogicEntity> InnerGetGroup();
        protected abstract void UpdateEntities(List<LogicEntity> entities, float dt);
        protected abstract void LateUpdateEntities(List<LogicEntity> entities, float dt);

        public void LateUpdate()
        {
            float dt = Time.deltaTime;

            if (_isEntitiesCacheDirty)
            {
                _entitiesCache.Clear();
                foreach (var entity in _group)
                {
                    _entitiesCache.Add(entity);
                }

                _isEntitiesCacheDirty = false;
            }

            LateUpdateEntities(_entitiesCache, dt);
        }
    }
}
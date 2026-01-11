using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public abstract class SysGroupTickBase : IExecuteSystem, ITearDownSystem, IInitializeSystem, ILateUpdateSystem
    {
        protected readonly LogicWorld mLogicWorld;
        protected IGroup<LogicEntity> mGroup;
        
        //Group.GetEntities 2种接口都会 new一个临时数组，这里针对GC优化一下
        protected List<LogicEntity> mEntitiesCache;
        protected bool mIsEntitiesCacheDirty;
        
        public SysGroupTickBase(LogicWorld logicWorld)
        {
            mLogicWorld = logicWorld;
            mEntitiesCache = new(32);
            mIsEntitiesCacheDirty = true;
        }
        
        public void Initialize()
        {
            mGroup = InnerGetGroup();
            mGroup.OnEntityAdded += onEntityAdded;
            mGroup.OnEntityRemoved += onEntityRemoved;
        }
        
        public void TearDown()
        {
            mGroup.OnEntityAdded -= onEntityAdded;
            mGroup.OnEntityRemoved -= onEntityRemoved;
        }
        
        protected void onEntityAdded(IGroup<LogicEntity> group, LogicEntity entity, int index, IComponent component)
        {
            mIsEntitiesCacheDirty = true;
        }
        protected void onEntityRemoved(IGroup<LogicEntity> group, LogicEntity entity, int index, IComponent component) 
        {
            mIsEntitiesCacheDirty = true;
        }
        
        public void Execute()
        {
            float dt = Time.deltaTime;

            if (mIsEntitiesCacheDirty)
            {
                mEntitiesCache.Clear();
                foreach (var entity in mGroup)
                {
                    mEntitiesCache.Add(entity);
                }
                mIsEntitiesCacheDirty = false;
            }
            UpdateEntities(mEntitiesCache, dt);
        }

        
        protected abstract IGroup<LogicEntity> InnerGetGroup();
        protected abstract void UpdateEntities(List<LogicEntity> entities, float dt);
        protected abstract void LateUpdateEntities(List<LogicEntity> entities, float dt);
        public void LateUpdate()
        {
            float dt = Time.deltaTime;
            
            if (mIsEntitiesCacheDirty)
            {
                mEntitiesCache.Clear();
                foreach (var entity in mGroup)
                {
                    mEntitiesCache.Add(entity);
                }
                mIsEntitiesCacheDirty = false;
            }
            LateUpdateEntities(mEntitiesCache, dt);
        }
    }
}
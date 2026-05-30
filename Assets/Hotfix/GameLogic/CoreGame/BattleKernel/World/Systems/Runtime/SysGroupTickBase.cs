using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    //目前说是不通用的
    public abstract class SysGroupTickBase<T> : IExecuteSystem, ITearDownSystem, IInitializeSystem, ILateUpdateSystem where T : Entity
    {
        protected readonly ECWorlds _worlds;
        protected IGroup<T> mGroup;

        //Group.GetEntities 2种接口都会 new一个临时数组，这里针对GC优化一下
        protected List<T> mEntitiesCache;
        protected bool mIsEntitiesCacheDirty;

        public SysGroupTickBase(ECWorlds worlds)
        {
            _worlds = worlds;
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

        protected void onEntityAdded(IGroup<T> group, T entity, int index, IComponent component)
        {
            mIsEntitiesCacheDirty = true;
        }

        protected void onEntityRemoved(IGroup<T> group, T entity, int index, IComponent component)
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

        protected abstract IGroup<T> InnerGetGroup();
        protected abstract void UpdateEntities(List<T> entities, float dt);
        protected abstract void LateUpdateEntities(List<T> entities, float dt);

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

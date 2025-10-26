using UnityEngine;

namespace LccHotfix
{
    public abstract class GameObjectPoolDecorator : IGameObjectPool
    {
        private IGameObjectPool _pool;
        protected IGameObjectPool Pool => _pool;

        public GameObjectPoolSetting PoolSetting => Pool.PoolSetting;
        public string Name => Pool.Name;
        public GameObject Root => Pool.Root;
        public int Count => Pool.Count;

        public GameObjectPoolDecorator(IGameObjectPool pool)
        {
            Debug.Assert(pool != null);
            _pool = pool;
        }

        public virtual GameObjectPoolObject Get()
        {
            var target = Pool.Get();
            if (target != null)
            {
                target.Pool = this;
            }

            return target;
        }

        public virtual void Release(GameObjectPoolObject poolObject)
        {
            Pool.Release(poolObject);
        }

        public virtual void ReleaseAll()
        {
            Pool.ReleaseAll();
        }

        public virtual void Update()
        {
            Pool.Update();
        }

        public GameObjectPoolObject ForceSpawm()
        {
            return Pool.ForceSpawm();
        }

        public void ForceRelease()
        {
            Pool.ForceRelease();
        }
    }
}
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 有最大数量限制的对象池
    /// 如果申请超过最大数量的对象，则会返回最先申请的对象
    /// </summary>
    public class MaxActiveDecorator : GameObjectPoolDecorator
    {
        private LinkedList<GameObjectPoolObject> _activeList = new LinkedList<GameObjectPoolObject>();

        public MaxActiveDecorator(IGameObjectPool pool) : base(pool)
        {
        }

        public override GameObjectPoolObject Get()
        {
            //这里的实现逻辑是先强制回收第一个,再取
            if (_activeList.Count >= PoolSetting.maxActiveObjects)
            {
                _activeList.First.Value.Release();
            }

            var target = base.Get();
            _activeList.AddLast(target);
            return target;
        }

        public override void Release(GameObjectPoolObject poolObject)
        {
            _activeList.Remove(poolObject);
            base.Release(poolObject);
        }
    }
}
namespace LccHotfix
{
    /// <summary>
    /// 有最大数量限制的对象池
    /// 如果申请超过最大数量的对象，则会返回最先申请的对象
    /// </summary>
    public class MaxActiveDecorator : GameObjectPoolDecorator
    {
        private System.Collections.Generic.LinkedList<GameObjectObject> _activeList = new System.Collections.Generic.LinkedList<GameObjectObject>();

        public MaxActiveDecorator(IGameObjectPool pool) : base(pool)
        {
        }

        public override GameObjectObject Get()
        {
            //这里的实现逻辑是先强制回收第一个,再取
            if (_activeList.Count >= PoolSetting.maxActiveObjects)
            {
                var obj = _activeList.First.Value;
                obj.Release(ref obj);
            }

            var target = base.Get();
            _activeList.AddLast(target);
            return target;
        }

        public override void Release(GameObjectObject obj)
        {
            _activeList.Remove(obj);
            base.Release(obj);
        }
    }
}
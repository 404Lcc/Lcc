namespace LccHotfix
{
    public class SetParentDecorator : GameObjectPoolDecorator
    {
        public SetParentDecorator(IGameObjectPool pool) : base(pool)
        {

        }

        public override void Release(GameObjectPoolObject poolObject)
        {
            base.Release(poolObject);
            if (poolObject != null)
            {
                poolObject.GameObject.transform.SetParent(Root.transform);
            }
        }
    }
}
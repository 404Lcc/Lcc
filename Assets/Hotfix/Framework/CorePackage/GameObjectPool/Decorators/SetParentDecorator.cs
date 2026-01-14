namespace LccHotfix
{
    public class SetParentDecorator : GameObjectPoolDecorator
    {
        public SetParentDecorator(IGameObjectPool pool) : base(pool)
        {

        }

        public override void Release(GameObjectObject obj)
        {
            base.Release(obj);
            if (obj != null)
            {
                obj.GameObject.transform.SetParent(Root.transform);
            }
        }
    }
}
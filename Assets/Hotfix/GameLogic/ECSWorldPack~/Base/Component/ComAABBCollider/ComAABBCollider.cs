namespace LccHotfix
{
    public interface IAABBColliderHandler
    {
        void CheckHits(LogicEntity ownerEntity);
        void Dispose();
    }

    public class ComAABBCollider : LogicComponent
    {
        public bool isActive;

        public IAABBColliderHandler handler;

        public override void Dispose()
        {
            base.Dispose();
            if (handler != null)
            {
                handler.Dispose();
                handler = null;
            }
        }
    }


    public partial class LogicEntity
    {
        public ComAABBCollider ComAABBCollider
        {
            get { return (ComAABBCollider)GetComponent(LogicComponentsLookup.ComAABBCollider); }
        }

        public bool hasComAABBCollider
        {
            get { return HasComponent(LogicComponentsLookup.ComAABBCollider); }
        }

        public void AddComAABBCollider(IAABBColliderHandler newHandler)
        {
            var index = LogicComponentsLookup.ComAABBCollider;
            var component = (ComAABBCollider)CreateComponent(index, typeof(ComAABBCollider));
            component.isActive = true;
            component.handler = newHandler;
            AddComponent(index, component);
        }

        public void ReplaceComAABBCollider(bool newIsActive, IAABBColliderHandler newHandler)
        {
            var index = LogicComponentsLookup.ComAABBCollider;
            var component = (ComAABBCollider)CreateComponent(index, typeof(ComAABBCollider));
            component.isActive = newIsActive;
            component.handler = newHandler;
            ReplaceComponent(index, component);
        }

        public void RemoveComAABBCollider()
        {
            RemoveComponent(LogicComponentsLookup.ComAABBCollider);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComAABBColliderIndex = new ComponentTypeIndex(typeof(ComAABBCollider));
        public static int ComAABBCollider => ComAABBColliderIndex.index;
    }
}
namespace LccHotfix
{
    public enum HitMethod
    {
        Collider2D,
        Raycast2D,
        BoxCollider,
        Raycast,
    }

    public interface IEntityColliderHandler : IDispose
    {
        //生成生数据
        bool CheckRawHits(LogicEntity ownerEntity, float dt);

        //处理生数据
        void HandleRawHits(LogicEntity ownerEntity, float dt);

        //清理当前帧数据
        void Cleanup();
    }

    public class ComCollider : LogicComponent
    {
        public bool isActive;

        public IEntityColliderHandler handler;

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

        public ComCollider comCollider
        {
            get { return (ComCollider)GetComponent(LogicComponentsLookup.ComCollider); }
        }

        public bool hasComCollider
        {
            get { return HasComponent(LogicComponentsLookup.ComCollider); }
        }

        public void AddComCollider(IEntityColliderHandler newHandler)
        {
            var index = LogicComponentsLookup.ComCollider;
            var component = (ComCollider)CreateComponent(index, typeof(ComCollider));
            component.isActive = true;
            component.handler = newHandler;
            AddComponent(index, component);
        }

        public void ReplaceComCollider(bool newIsActive, IEntityColliderHandler newHandler)
        {
            var index = LogicComponentsLookup.ComCollider;
            var component = (ComCollider)CreateComponent(index, typeof(ComCollider));
            component.isActive = newIsActive;
            component.handler = newHandler;
            ReplaceComponent(index, component);
        }

        public void RemoveComCollider()
        {
            RemoveComponent(LogicComponentsLookup.ComCollider);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComColliderIndex = new ComponentTypeIndex(typeof(ComCollider));
        public static int ComCollider => ComColliderIndex.index;
    }
}
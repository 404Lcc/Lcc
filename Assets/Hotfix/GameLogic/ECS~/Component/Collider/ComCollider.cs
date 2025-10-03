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

        public ComCollider comCollider { get { return (ComCollider)GetComponent(LogicComponentsLookup.ComCollider); } }
        public bool hasComCollider { get { return HasComponent(LogicComponentsLookup.ComCollider); } }

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
    public sealed partial class LogicMatcher
    {

        private static Entitas.IMatcher<LogicEntity> _matcherComCollider;

        public static Entitas.IMatcher<LogicEntity> ComCollider
        {
            get
            {
                if (_matcherComCollider == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComCollider);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComCollider = matcher;
                }

                return _matcherComCollider;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComCollider;
    }
}
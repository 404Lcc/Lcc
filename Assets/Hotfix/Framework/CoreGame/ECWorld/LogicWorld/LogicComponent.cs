using Entitas;

namespace LccHotfix
{
    public class LogicComponent : IComponent, IComponentDispose
    {
        protected LogicEntity _owner;

        public LogicEntity Owner => _owner;

        public virtual void PostInitialize(LogicEntity owner)
        {
            _owner = owner;
        }

        public virtual void DisposeOnRemove()
        {
            _owner = null;
        }

        public static bool operator !(LogicComponent component)
        {
            return component == null;
        }

        public static bool operator true(LogicComponent component)
        {
            return component != null;
        }

        public static bool operator false(LogicComponent component)
        {
            return component == null;
        }
    }
}
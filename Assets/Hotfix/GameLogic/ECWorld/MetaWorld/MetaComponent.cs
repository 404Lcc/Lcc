using Entitas;

namespace LccHotfix
{
    public class MetaComponent : IComponent, IComponentDispose
    {
        private MetaEntity _owner;
        public MetaEntity Owner => _owner;

        public virtual void PostInitialize(MetaEntity owner)
        {
            _owner = owner;
        }

        public virtual void DisposeOnRemove()
        {
            _owner = null;
        }

        public static bool operator !(MetaComponent component)
        {
            return component == null;
        }

        public static bool operator true(MetaComponent component)
        {
            return component != null;
        }

        public static bool operator false(MetaComponent component)
        {
            return component == null;
        }
    }
}
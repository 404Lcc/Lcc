using Entitas;

namespace LccHotfix
{
    public class MetaComponent : IComponent, IDispose
    {
        private MetaEntity _owner;
        public MetaEntity Owner => _owner;

        public virtual void PostInitialize(MetaEntity owner)
        {
            _owner = owner;
        }

        public virtual void Dispose()
        {
            _owner = null;
        }
    }
}
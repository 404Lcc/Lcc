using Entitas;
using LccHotfix;

namespace LccHotfix
{
    public class SysOrca : IExecuteSystem
    {
        private MetaContext _metaContext;

        public SysOrca(ECSWorld world)
        {
            _metaContext = world.MetaContext;
        }

        public void Execute()
        {
            _metaContext.ComUniOrca.DoStep();
            _metaContext.ComUniOrca.EnsureCompleted();
        }
    }
}
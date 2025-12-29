using Entitas;

namespace LccHotfix
{
    public class SysJobUpdate : IExecuteSystem
    {
        private MetaContext _metaContext;

        public SysJobUpdate(ECSWorld world)
        {
            _metaContext = world.MetaContext;
        }

        public void Execute()
        {
            if (_metaContext.hasComUniJob)
            {
                _metaContext.ComUniJob.Update();
            }
        }
    }
}
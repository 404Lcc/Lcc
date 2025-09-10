using Entitas;
using LccHotfix;

namespace LccHotfix
{
    public class SysOrca : IExecuteSystem
    {
        private ComUniOrca comUniOrca;

        public SysOrca(ECSWorld world)
        {
            comUniOrca = world.MetaContext.ComUniOrca;
        }

        public void Execute()
        {
            comUniOrca.DoStep();
            comUniOrca.EnsureCompleted();
        }
    }
}
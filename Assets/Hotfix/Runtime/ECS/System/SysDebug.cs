using Entitas;

namespace LccHotfix
{
    public class SysDebug : IExecuteSystem, ITearDownSystem
    {
        private ECSWorld _world;

        public SysDebug(ECSWorld world)
        {
            _world = world;
        }

        public void TearDown()
        {

        }

        public void Execute()
        {

        }

    }
}
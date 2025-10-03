using Entitas;

namespace LccHotfix
{
    public class SysGameMode : IExecuteSystem, IInitializeSystem, ITearDownSystem
    {
        private MetaContext _metaContext;

        public SysGameMode(ECSWorld world)
        {
            _metaContext = world.MetaContext;
        }

        public void Initialize()
        {
            _metaContext.ComUniGameMode.mode.Start();
        }

        public void TearDown()
        {
            _metaContext.ComUniGameMode.mode.Release();
        }


        public void Execute()
        {
            _metaContext.ComUniGameMode.mode.Update();
        }
    }
}
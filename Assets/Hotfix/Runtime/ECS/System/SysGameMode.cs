using Entitas;

namespace LccHotfix
{
    public class SysGameMode : IExecuteSystem, IInitializeSystem, ITearDownSystem
    {
        private GameModeBase _gameMode;

        public SysGameMode(ECSWorld world)
        {
            _gameMode = world.MetaContext.comUniGameMode.mode;
        }

        public void Initialize()
        {
            _gameMode.Start();
        }

        public void TearDown()
        {
            _gameMode.Release();
        }


        public void Execute()
        {
            _gameMode.Update();
        }
    }
}
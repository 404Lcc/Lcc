using Entitas;

namespace LccHotfix
{
    public class SysGameModeUpdate : SysBase, IInitializeSystem, IExecuteSystem, ITearDownSystem
    {
        public SysGameModeUpdate(ECWorlds world) : base(world)
        {
        }
        
        public void Initialize()
        {
            
        }

        public void Execute()
        {
            var comUniGameMode = _world.MetaWorld.comUniGameMode;
            if (comUniGameMode)
            {
                var dt = BattleTime.GetDeltaTime(_logicWorld);
                var modeDt = dt;
                var modeLogic = comUniGameMode.GameModeLogic;
                modeLogic.Update(modeDt);
            }
        }

        public void TearDown()
        {
            _world.MetaWorld.RemoveComUniGameMode();
        }
    }
}

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
                float dt = BattleTime.GetDeltaTime(_world.LogicWorld);    
                var modeLogic = comUniGameMode.GameModeLogic;
                modeLogic.Update(dt);
            }
        }

        public void TearDown()
        {
            _world.MetaWorld.RemoveComUniGameMode();
        }
    }
}

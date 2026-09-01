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
                var mainHero = _logicWorld.GetLogicGroup_Hero();
                var modeDt = dt * BattleBulletTimeUtility.GetCompensateRatio(mainHero, _metaWorld);
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

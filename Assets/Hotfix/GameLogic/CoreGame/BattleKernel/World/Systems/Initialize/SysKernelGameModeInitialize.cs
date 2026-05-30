using Entitas;

namespace LccHotfix
{
    public class SysKernelGameModeInitialize : SysBase, IInitializeSystem, ITearDownSystem
    {
        public SysKernelGameModeInitialize(ECWorlds world) : base(world)
        {
        }

        public void Initialize()
        {
            var creationInfo = _world.GetCreationInfo<BattleKernelCreationInfo>();
            if (creationInfo.GameModeGenInfo == null)
            {
                BattleLog.Error("SysKernelGameModeInitialize GameModeGenInfo == null");
                return;
            }

            if (creationInfo.ModeLogicService == null)
            {
                BattleLog.Error("SysKernelGameModeInitialize ModeLogicService == null");
                return;
            }

            _world.MetaWorld.SetComUniGameMode(creationInfo.GameModeGenInfo, creationInfo.ModeLogicService);
        }

        public void TearDown()
        {
            _world.MetaWorld.RemoveComUniGameMode();
        }
    }
}

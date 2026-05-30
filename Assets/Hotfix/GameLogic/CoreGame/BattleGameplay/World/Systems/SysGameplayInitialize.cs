using Entitas;

namespace LccHotfix
{
    public class SysGameplayInitialize : SysBase, IInitializeSystem
    {
        public SysGameplayInitialize(ECWorlds world) : base(world)
        {
        }

        public void Initialize()
        {
            var creationInfo = _world.GetCreationInfo<ECGameWorldCreationInfo>();
            DemoBattleConfigRegister.Register();
            InitGameModeEnv(creationInfo);
        }

        private void InitGameModeEnv(ECGameWorldCreationInfo creationInfo)
        {
            var genInfo = creationInfo.GameModeGenInfo;
            if (genInfo == null)
            {
                return;
            }

            var varEnv = genInfo.PreEnv ?? new VarEnv();
            varEnv.WriteVar(CvKey.CV_WorldInfo, creationInfo);
            varEnv.WriteVar(CvKey.CV_LogicWorld, _world.LogicWorld);
            varEnv.WriteVar(CvKey.CV_MetaWorld, _world.MetaWorld);
            genInfo.PreEnv = varEnv;
        }
    }
}

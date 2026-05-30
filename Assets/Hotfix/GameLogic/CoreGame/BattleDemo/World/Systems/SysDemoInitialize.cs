using Entitas;

namespace LccHotfix
{
    public class SysDemoInitialize : SysBase, IInitializeSystem
    {
        public SysDemoInitialize(ECWorlds world) : base(world)
        {
        }

        public void Initialize()
        {
            var creationInfo = _world.GetCreationInfo<DemoWorldCreationInfo>();
            DemoBattleConfigRegister.Register();
            InitGameModeEnv(creationInfo);
        }

        private void InitGameModeEnv(DemoWorldCreationInfo creationInfo)
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

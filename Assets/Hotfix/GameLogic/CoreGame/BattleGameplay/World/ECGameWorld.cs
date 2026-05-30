namespace LccHotfix
{
    public class ECGameWorld : BattleKernelWorld
    {
        protected override void AddExternalInitializeSystems(ECSystems systems)
        {
            systems.Add(new SysGameplayInitialize(this));
        }

        public static ECGameWorld CreateWorld(IWorldCreationInfo creationInfo)
        {
            var world = new ECGameWorld();
            world.InitWorlds(creationInfo);
            return world;
        }
    }
}

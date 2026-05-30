namespace LccHotfix
{
    public class DemoWorld : BattleKernelWorld
    {
        protected override void AddExternalInitializeSystems(ECSystems systems)
        {
            systems.Add(new SysDemoInitialize(this));
        }

        public static DemoWorld CreateWorld(IWorldCreationInfo creationInfo)
        {
            var world = new DemoWorld();
            world.InitWorlds(creationInfo);
            return world;
        }
    }
}

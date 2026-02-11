namespace LccHotfix
{
    public class ECGameWorld : ECWorlds
    {
        protected override void CreateSystems()
        {
            mRootSystem = new ECSystems();

        }

        public static bool operator !(ECGameWorld world)
        {
            return world == null;
        }

        public static bool operator true(ECGameWorld world)
        {
            return world != null;
        }

        public static bool operator false(ECGameWorld world)
        {
            return world == null;
        }

        public static ECGameWorld CreateWorld(IWorldCreationInfo creationInfo)
        {
            var world = new ECGameWorld();
            world.InitWorlds(creationInfo);
            return world;
        }
    }
}
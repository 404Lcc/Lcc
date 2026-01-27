namespace LccHotfix
{
    public class SysBase
    {
        protected ECWorlds _world;

        protected MetaWorld _metaWorld => _world.MetaWorld;
        protected LogicWorld _logicWorld => _world.LogicWorld;
        
        public SysBase(ECWorlds world)
        {
            _world = world;
        }
    }
}
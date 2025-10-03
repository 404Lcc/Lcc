using System;

namespace LccHotfix
{
    internal class WorldManager : Module, IWorldService
    {
        private ECSWorld _world;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_world == null)
                return;
            _world.Update();
        }

        internal override void LateUpdate()
        {
            base.LateUpdate();

            if (_world == null)
                return;
            _world.LateUpdate();
        }

        internal override void Shutdown()
        {
            if (_world == null)
                return;

            _world.DestroyWorld();
            _world = null;
        }

        public void CreateWorld<T>(IWorldData data) where T : ECSWorld
        {
            _world = Activator.CreateInstance<T>();
            _world.InitWorld(data);
        }

        public ECSWorld GetWorld()
        {
            if (_world == null)
                return null;
            return _world;
        }

        public T GetWorld<T>() where T : ECSWorld
        {
            if (_world == null)
                return null;
            return _world as T;
        }

        public void ExitWorld()
        {
            if (_world == null)
                return;
            _world.DestroyWorld();
            _world = null;
        }
    }
}
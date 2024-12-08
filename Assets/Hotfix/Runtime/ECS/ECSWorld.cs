using Entitas;
using LccModel;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ECSWorld
    {
        private bool _pause = false;

        public bool IsPause => _pause;
        public List<IContext> ContextList { get; private set; }
        public LogicSystems System { get; private set; }

        public virtual void InitWorld()
        {
            ContextList = new List<IContext>();
            ContextList.Add(new LogicContext());
            ContextList.Add(new MetaContext());
            InitializeEntityIndices();
            InitComponent();
            System = new LogicSystems();
            InitSystem();
            System.Initialize();
            System.ActivateReactiveSystems();
        }

        protected virtual void InitializeEntityIndices()
        {
        }

        protected virtual void InitComponent()
        {
        }

        protected virtual void InitSystem()
        {
        }

        public virtual void PauseGame()
        {
            _pause = true;
            Launcher.Instance.Pause();
        }

        public virtual void ResumeGame()
        {
            if (_pause)
            {
                _pause = false;
                Launcher.Instance.Resume();
            }
        }

        public virtual void Update()
        {
            if (System != null)
            {
                System.Execute();
                System.Cleanup();
            }
        }

        public virtual void LateUpdate()
        {
            if (System != null)
            {
                System.LateUpdate();
            }
        }

        public virtual void Exit()
        {
            if (System != null)
            {
                System.Cleanup();
                System.TearDown();
                System.DeactivateReactiveSystems();
                System.ClearReactiveSystems();

                System = null;
            }

            foreach (var item in ContextList)
            {
                item.DestroyAllEntities();
                item.Reset();
            }
        }
    }
}
using Entitas;
using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ECSWorld
    {
        private bool _pause = false;
        protected GameModeState _gameModeState;

        public bool IsPause => _pause;
        public LogicContext LogicContext { get; private set; }
        public MetaContext MetaContext { get; private set; }
        public List<IContext> ContextList { get; private set; }
        public LogicSystems System { get; private set; }

        public virtual void InitWorld(GameModeState gameModeState)
        {
            _gameModeState = gameModeState;
            LogicComponentsLookup.componentTypeList = new List<Type>();
            MetaComponentsLookup.componentTypeList = new List<Type>();
            Setup();
            LogicComponentsLookup.componentNameList = new List<string>();
            MetaComponentsLookup.componentNameList = new List<string>();
            foreach (var item in LogicComponentsLookup.componentTypes)
            {
                LogicComponentsLookup.componentNameList.Add(item.Name);
            }
            foreach (var item in MetaComponentsLookup.componentTypes)
            {
                MetaComponentsLookup.componentNameList.Add(item.Name);
            }

            ContextList = new List<IContext>();
            LogicContext = new LogicContext();
            MetaContext = new MetaContext();
            ContextList.Add(LogicContext);
            ContextList.Add(MetaContext);
            InitializeEntityIndices();
            InitComponent();
            System = new LogicSystems();
            InitSystem();
            System.Initialize();
            System.ActivateReactiveSystems();
        }

        protected virtual void Setup()
        {
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

        public virtual void Pause()
        {
            _pause = true;
            Launcher.Instance.GameControl.Pause();
        }

        public virtual void Resume()
        {
            if (_pause)
            {
                _pause = false;
                Launcher.Instance.GameControl.Resume();
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

        public IEntityIndex GetEntityIndex<T>()
        {
            return LogicContext.GetEntityIndex(typeof(T).Name);
        }
        public TIndex GetEntityIndex<TComponent, TIndex>()
        {
            return (TIndex)GetEntityIndex<TComponent>();
        }
    }
}
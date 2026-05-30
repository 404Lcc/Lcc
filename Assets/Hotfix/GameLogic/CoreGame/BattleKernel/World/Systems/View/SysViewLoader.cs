using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysViewLoader : ReactiveSystem<LogicEntity>
    {
        private static ECWorlds _worlds;

        public SysViewLoader(ECWorlds world) : base(world.LogicWorld)
        {
            _worlds = world;
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return new Collector<LogicEntity>(
                new IGroup<LogicEntity>[]
                {
                    context.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComViewLoader))
                },
                new GroupEvent[]
                {
                    GroupEvent.Added,
                }
            );
        }

        protected override bool Filter(LogicEntity entity)
        {
            return entity.hasComViewLoader;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var entity in entities)
            {
                var comViewLoader = entity.ComViewLoader;
                var viewData = comViewLoader.mainViewData;
                LoadView(entity, viewData);
            }
        }

        private void LoadView(LogicEntity entity, IViewLoader loaderData)
        {
            AddView(entity, loaderData);
            if (loaderData.SubLoaderList != null)
            {
                for (int i = 0; i < loaderData.SubLoaderList.Count; i++)
                {
                    LoadView(entity, loaderData.SubLoaderList[i]);
                }
            }
        }

        #region Common

        private void AddView(LogicEntity entity, IViewLoader loaderData)
        {
            if (loaderData.IsPrepare)
                return;
            var category = loaderData.Category;
            if (entity.hasComView)
            {
                var comView = entity.comView;
                if (comView.HasView(category))
                {
                    comView.RemoveView(category);
                }
            }

            loaderData.Load(entity, HandleLoaded);
        }

        private void HandleLoaded(LogicEntity entity, int category, IReceiveLoaded loaded)
        {
            if (entity.hasComViewLoader)
            {
                entity.ComViewLoader.ReceiveLoaded(category, loaded, entity, _worlds);
            }
            else
            {
                loaded.Dispose();
            }
        }

        #endregion
    }
}
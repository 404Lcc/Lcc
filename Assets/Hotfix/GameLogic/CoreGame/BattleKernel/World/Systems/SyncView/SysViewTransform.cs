using UnityEngine;
using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SysSyncViewTransform : ReactiveSystem<LogicEntity>
    {
        public SysSyncViewTransform(ECWorlds worlds) : base(worlds.LogicWorld)
        {
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return new Collector<LogicEntity>(
                new IGroup<LogicEntity>[]
                {
                    context.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComView, LogicComponentsLookup.ComTransform))
                },
                new GroupEvent[]
                {
                    GroupEvent.AddedOrRemoved,
                }
            );
        }

        protected override bool Filter(LogicEntity entity)
        {
            return entity.hasComView && entity.hasComTransform;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var entity in entities)
            {
                var position = entity.position;
                var rotation = entity.rotation;
                var scale = entity.scale;


                if (float.IsNaN(position.x))
                {
                    UnityEngine.Debug.LogWarning($"SysSyncViewTransform: entityID={entity.ID} position X is NaN");
                    continue;
                }

                if (float.IsNaN(position.z))
                {
                    UnityEngine.Debug.LogWarning($"SysSyncViewTransform: entityID={entity.ID} position Z is NaN");
                    continue;
                }

                var comView = entity.comView;
                var viewList = comView.ViewList;
                var scaleVector = new Vector3(scale.x, scale.y, scale.z);
                var mainView = comView.GetView<IViewWrapper>(EViewCategory.MainGameObject);
                mainView?.SyncTransform(entity.ID, position, rotation, scaleVector);

                for (int i = 0; i < viewList.Count; i++)
                {
                    var viewWrapper = viewList[i];
                    if (viewWrapper.Category == EViewCategory.MainGameObject)
                    {
                        continue;
                    }

                    // 屏幕空间 UI（血条/弹药等）只在相机 LateUpdate 之后投影，Execute 阶段跳过
                    if (viewWrapper.IsScreenSpaceUI)
                    {
                        continue;
                    }

                    viewWrapper.SyncTransform(entity.ID, position, rotation, scaleVector);
                }
            }
        }
    }
}

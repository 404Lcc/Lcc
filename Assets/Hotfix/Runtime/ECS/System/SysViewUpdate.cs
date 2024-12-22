using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysViewUpdate : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;
        public SysViewUpdate(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComView));
        }


        public void Execute()
        {
            float dt = Time.deltaTime;
            foreach (var entity in _group.GetEntities())
            {
                var comView = entity.comView;
                var viewList = comView.ViewList;
                for (int i = viewList.Count - 1; i >= 0; i--)
                {
                    var viewNeedUpdate = viewList[i] as IViewUpdate;
                    if (viewNeedUpdate != null)
                    {
                        viewNeedUpdate.Update(dt);
                    }
                }
            }
        }
    }
}
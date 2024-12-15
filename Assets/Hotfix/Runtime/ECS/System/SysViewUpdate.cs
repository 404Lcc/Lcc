using Entitas;

namespace LccHotfix
{
    public class SysViewUpdate : IExecuteSystem
    {
        private readonly ECSWorld _contexts;
        private readonly IGroup<LogicEntity> _group;
        public SysViewUpdate(ECSWorld contexts)
        {
            _contexts = contexts;

            _group = _contexts.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComView));
        }


        public void Execute()
        {
            var dt = UnityEngine.Time.deltaTime;
            var entities = _group.GetEntities();
            foreach (var entity in entities)
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
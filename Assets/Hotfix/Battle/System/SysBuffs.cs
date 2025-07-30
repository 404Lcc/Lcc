using System.Collections.Generic;
using UnityEngine;
using Entitas;

namespace LccHotfix
{
    public class SysBuffs : IExecuteSystem, ITearDownSystem
    {
        private IGroup<LogicEntity> _group;

        private List<int> _removeList = new List<int>();
        public SysBuffs(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComBuffs));
        }

        public void TearDown()
        {
            _removeList.Clear();
        }

        public void Execute()
        {
            var dt = Time.deltaTime;

            foreach (var entity in _group.GetEntities())
            {
                var comBuffs = entity.comBuffs;
                foreach (var item in comBuffs.BuffDict.Values)
                {
                    item.UpdateState(dt);

                    if (!item.IsActive)
                    {
                        _removeList.Add(item.BuffId);
                    }
                }

                foreach (var buffId in _removeList)
                {
                    if (comBuffs.BuffDict.TryGetValue(buffId, out var buffState))
                    {
                        buffState.LeaveState();
                        comBuffs.BuffDict.Remove(buffId);
                    }
                }
                _removeList.Clear();
            }
        }
    }
}
using System.Collections.Generic;
using System;

namespace LccModel
{
    /// <summary>
    /// 条件管理组件，在这里管理一个战斗实体所有条件达成事件的添加监听、移除监听、触发流程
    /// </summary>
    public sealed class ConditionComponent : Component
    {
        private Dictionary<Action, ConditionEntity> Conditions { get; set; } = new Dictionary<Action, ConditionEntity>();


        public void AddListener(ConditionType conditionType, Action action, object paramObj = null)
        {
            switch (conditionType)
            {
                case ConditionType.WhenInTimeNoDamage:
                    var time = (float)paramObj;
                    var condition = Parent.AddChildren<ConditionEntity>();
                    var comp = condition.AddComponent<ConditionWhenInTimeNoDamageComponent, float>(time);
                    Conditions.Add(action, condition);
                    comp.StartListen(action);
                    break;
                case ConditionType.WhenHPLower:
                    break;
                case ConditionType.WhenHPPctLower:
                    break;
                default:
                    break;
            }
        }

        public void RemoveListener(ConditionType conditionType, Action action)
        {
            if (Conditions.ContainsKey(action))
            {
                Conditions[action].Dispose();
                Conditions.Remove(action);
            }
        }
    }
}
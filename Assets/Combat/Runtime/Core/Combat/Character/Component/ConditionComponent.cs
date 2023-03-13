using System.Collections.Generic;
using System;

namespace LccModel
{
    public class ConditionComponent : Component
    {
        private Dictionary<long, Condition> _conditionDict = new Dictionary<long, Condition>();


        public void AddListener(ConditionType type, Action action, object obj = null)
        {
            switch (type)
            {
                case ConditionType.WhenInTimeNoDamage:
                    var time = float.Parse((string)obj);
                    var condition = Parent.AddChildren<Condition>();
                    var temp = condition.AddComponent<ConditionWhenInTimeNoDamageComponent, float>(time);
                    temp.StartListen(action);
                    _conditionDict.Add(action.GetHashCode(), condition);
                    break;
                case ConditionType.WhenHPLower:
                    break;
                case ConditionType.WhenHPPctLower:
                    break;
                default:
                    break;
            }
        }

        public void RemoveListener(ConditionType type, Action action)
        {
            if (_conditionDict.ContainsKey(action.GetHashCode()))
            {
                _conditionDict[action.GetHashCode()].Dispose();
                _conditionDict.Remove(action.GetHashCode());
            }
        }
    }
}
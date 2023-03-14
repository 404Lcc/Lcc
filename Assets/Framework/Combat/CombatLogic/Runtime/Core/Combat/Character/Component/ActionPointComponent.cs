using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

namespace LccModel
{
    [Flags]
    public enum ActionPointType
    {
        [LabelText("（空）")]
        None = 0,

        [LabelText("造成伤害前")]
        PreCauseDamage = 1 << 1,
        [LabelText("承受伤害前")]
        PreReceiveDamage = 1 << 2,

        [LabelText("造成伤害后")]
        PostCauseDamage = 1 << 3,
        [LabelText("承受伤害后")]
        PostReceiveDamage = 1 << 4,

        [LabelText("给予治疗后")]
        PostGiveCure = 1 << 5,
        [LabelText("接受治疗后")]
        PostReceiveCure = 1 << 6,

        [LabelText("赋给技能效果")]
        AssignEffect = 1 << 7,
        [LabelText("接受技能效果")]
        ReceiveEffect = 1 << 8,

        [LabelText("赋加状态后")]
        PostGiveStatus = 1 << 9,
        [LabelText("承受状态后")]
        PostReceiveStatus = 1 << 10,

        [LabelText("给予普攻前")]
        PreGiveAttack = 1 << 11,
        [LabelText("给予普攻后")]
        PostGiveAttack = 1 << 12,

        [LabelText("遭受普攻前")]
        PreReceiveAttack = 1 << 13,
        [LabelText("遭受普攻后")]
        PostReceiveAttack = 1 << 14,

        [LabelText("起跳前")]
        PreJumpTo = 1 << 15,
        [LabelText("起跳后")]
        PostJumpTo = 1 << 16,

        [LabelText("施法前")]
        PreSpell = 1 << 17,
        [LabelText("施法后")]
        PostSpell = 1 << 18,

        [LabelText("赋给普攻效果前")]
        PreGiveAttackEffect = 1 << 19,
        [LabelText("赋给普攻效果后")]
        PostGiveAttackEffect = 1 << 20,
        [LabelText("承受普攻效果前")]
        PreReceiveAttackEffect = 1 << 21,
        [LabelText("承受普攻效果后")]
        PostReceiveAttackEffect = 1 << 22,

        [LabelText("赋给物品前")]
        PreGiveItem = 1 << 23,
        [LabelText("赋给物品后")]
        PostGiveItem = 1 << 24,
        [LabelText("承受物品前")]
        PreReceiveItem = 1 << 25,
        [LabelText("承受物品后")]
        PostReceiveItem = 1 << 26,

        Max,
    }

    public class ActionPoint
    {
        private List<Action<Entity>> _listenerList = new List<Action<Entity>>();


        public void AddListener(Action<Entity> action)
        {
            _listenerList.Add(action);
        }

        public void RemoveListener(Action<Entity> action)
        {
            _listenerList.Remove(action);
        }

        public void TriggerActionPoint(Entity actionExecution)
        {
            if (_listenerList.Count == 0)
            {
                return;
            }
            for (int i = _listenerList.Count - 1; i >= 0; i--)
            {
                var item = _listenerList[i];
                item.Invoke(actionExecution);
            }
        }
    }


    public class ActionPointComponent : Component
    {
        private Dictionary<ActionPointType, ActionPoint> _actionPointDict = new Dictionary<ActionPointType, ActionPoint>();


        public void AddListener(ActionPointType actionPointType, Action<Entity> action)
        {
            if (!_actionPointDict.ContainsKey(actionPointType))
            {
                _actionPointDict.Add(actionPointType, new ActionPoint());
            }
            _actionPointDict[actionPointType].AddListener(action);
        }

        public void RemoveListener(ActionPointType actionPointType, Action<Entity> action)
        {
            if (_actionPointDict.ContainsKey(actionPointType))
            {
                _actionPointDict[actionPointType].RemoveListener(action);
            }
        }

        public ActionPoint GetActionPoint(ActionPointType actionPointType)
        {
            if (_actionPointDict.TryGetValue(actionPointType, out var actionPoint))
            {
                return actionPoint;
            }
            return null;
        }

        public void TriggerActionPoint(ActionPointType actionPointType, Entity actionExecution)
        {
            if (_actionPointDict.TryGetValue(actionPointType, out var actionPoint))
            {
                actionPoint.TriggerActionPoint(actionExecution);
            }
        }
    }
}
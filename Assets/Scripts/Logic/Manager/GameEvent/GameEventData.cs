using System;

namespace LccModel
{
    public class GameEventData
    {
        public int id = -1;
        public event Action Callback;
        public event Func<bool> Condition;
        public GameEventData()
        {
        }
        public GameEventData(int id, Action callback, Func<bool> condition)
        {
            this.id = id;
            Callback = callback;
            Condition = condition;
        }
        public void Excute()
        {
            Callback?.Invoke();
        }
        public bool IsCondition()
        {
            return (bool)Condition?.Invoke();
        }
        public void Reset()
        {
            id = -1;
            Callback = null;
            Condition = null;
        }
    }
}
using System;

namespace LccHotfix
{
    public class ActionWrap<T>
    {
        public Action<T> wrap;
        public Action action;
        public ActionWrap()
        {
        }
        public ActionWrap(Action action)
        {
            wrap = Execute;
            this.action = action;
        }
        public void Execute(T t)
        {
            action?.Invoke();
            wrap = null;
            action = null;
        }
        public static Action<T> Create(Action action)
        {
            ActionWrap<T> item = new ActionWrap<T>(action);
            return item.wrap;
        }
    }
}
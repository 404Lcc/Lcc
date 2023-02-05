using System;

namespace LccModel
{
    public class Component : AObjectBase
    {
        #region ÊÂ¼þ
        public T Publish<T>(T t) where T : class
        {
            ((Entity)Parent).Publish(t);
            return t;
        }
        public void Subscribe<T>(Action<T> action) where T : class
        {
            ((Entity)Parent).Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            ((Entity)Parent).UnSubscribe(action);
        }
        #endregion
    }
}
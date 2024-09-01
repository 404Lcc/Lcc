using LccModel;
using System;

namespace LccHotfix
{
    public abstract class AEvent<T> : IEvent
    {
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }
        protected abstract void Run(T data);

        public void Handle(T data)
        {
            try
            {
                Run(data);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
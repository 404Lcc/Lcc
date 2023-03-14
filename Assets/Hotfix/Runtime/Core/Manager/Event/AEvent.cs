using ET;
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
        protected abstract ETTask Run(T data);

        public async ETTask Handle(T data)
        {
            try
            {
                await Run(data);
            }
            catch (Exception e)
            {
                LogUtil.Error(e);
            }
        }
    }
}
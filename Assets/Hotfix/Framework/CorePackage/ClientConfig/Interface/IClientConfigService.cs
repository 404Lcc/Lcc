using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public interface IClientConfigService : IService
    {
        public T GetConfig<T>() where T : class, IClientConfig;
    }
}
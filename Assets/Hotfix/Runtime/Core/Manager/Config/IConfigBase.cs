using System;

namespace LccHotfix
{
    public interface IConfigBase
    {
        Type ConfigType
        {
            get;
        }
        void InitConfig();
    }
}
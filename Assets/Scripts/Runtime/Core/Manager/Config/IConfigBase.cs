using System;

namespace LccModel
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
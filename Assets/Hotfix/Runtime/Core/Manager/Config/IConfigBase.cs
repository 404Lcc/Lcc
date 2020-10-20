using System;

namespace Hotfix
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
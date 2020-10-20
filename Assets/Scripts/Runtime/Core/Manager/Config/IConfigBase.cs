using System;

namespace Model
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
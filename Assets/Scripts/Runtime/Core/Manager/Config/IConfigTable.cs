using System;

namespace LccModel
{
    public interface IConfigTable
    {
        Type ConfigType
        {
            get;
        }
        void InitConfigTable();
    }
}
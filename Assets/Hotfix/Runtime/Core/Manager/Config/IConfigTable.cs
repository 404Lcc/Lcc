using System;

namespace LccHotfix
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
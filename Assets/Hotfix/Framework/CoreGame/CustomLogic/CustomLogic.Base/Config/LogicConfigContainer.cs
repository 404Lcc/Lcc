using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public interface ILogicConfigContainer
    {
        string ContainerName { get; }
        CustomLogicCfg GetCustomLogicCfg(int id);
    }
}

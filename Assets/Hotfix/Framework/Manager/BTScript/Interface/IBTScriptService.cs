using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public interface IBTScriptService : IService
    {
        BTScript GetScript(string name);
    }
}
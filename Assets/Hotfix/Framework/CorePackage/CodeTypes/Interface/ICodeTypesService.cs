using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public interface ICodeTypesService : IService
    {
        void LoadTypes(Assembly[] assemblies);
        Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args);
        HashSet<Type> GetTypes(Type systemAttributeType);
        Dictionary<string, Type> GetTypes();
        Type GetType(string typeName);
    }
}
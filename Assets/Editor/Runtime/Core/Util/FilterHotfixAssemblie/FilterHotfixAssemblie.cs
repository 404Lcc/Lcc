using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace LccEditor
{
    public class FilterHotFixAssemblies : IFilterBuildAssemblies
    {
        public const string Hotfix = "Unity.Hotfix.dll";
        public int callbackOrder => 0;
        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            return assemblies.Where(dll => !dll.EndsWith(Hotfix, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }
}
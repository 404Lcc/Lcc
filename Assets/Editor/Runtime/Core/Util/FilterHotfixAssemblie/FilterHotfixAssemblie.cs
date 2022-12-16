using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LccEditor
{
    public class FilterHotFixAssemblies : IFilterBuildAssemblies
    {
        public const string Hotfix = "Unity.Hotfix.dll";
        public int callbackOrder => 0;
        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            foreach (var item in assemblies)
            {
                Debug.Log(item);
            }
            return assemblies.Where(dll => !Hotfix.EndsWith(dll, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }
}
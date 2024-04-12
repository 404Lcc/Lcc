using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LccEditor
{
    public class FilterHotFixAssemblies : IFilterBuildAssemblies
    {
        public int callbackOrder => 0;
        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            List<string> allHotUpdateDllNames = new List<string>() { "Unity.Hotfix.dll" };


            // 将热更dll从打包列表中移除
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = allHotUpdateDllNames.All(dll => !assName.Equals(dll, StringComparison.Ordinal));
                if (!reserved)
                {
                    Debug.Log($"[FilterHotFixAssemblies] filter assembly:{assName}");
                }
                return reserved;
            }).ToArray();
        }
    }
}
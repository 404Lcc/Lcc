using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class Preprocess : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    public void OnPreprocessBuild(BuildReport report)
    {
        if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
        {
            File.Delete("Library/ScriptAssemblies/Unity.Hotfix.dll");
            File.Delete("Library/ScriptAssemblies/Unity.Hotfix.pdb");
            Debug.Log("删除Hotfix.dll Hotfix.pdb");
        }
    }
}
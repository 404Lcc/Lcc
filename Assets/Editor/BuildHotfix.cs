using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BuildHotfix
{
    static BuildHotfix()
    {
        File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Resources/Text/Unity.Hotfix.dll.bytes", true);
        File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Resources/Text/Unity.Hotfix.pdb.bytes", true);
        Debug.Log("复制Hotfix.dll Hotfix.pdb");
        AssetDatabase.Refresh();
    }
}
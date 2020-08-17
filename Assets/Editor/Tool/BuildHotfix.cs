using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BuildHotfix
{
    static BuildHotfix()
    {
#if !Release
        if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
        {
            File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Resources/Text/Unity.Hotfix.dll.bytes", true);
            File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Resources/Text/Unity.Hotfix.pdb.bytes", true);
            Debug.Log("复制Hotfix.dll Hotfix.pdb");
            AssetDatabase.Refresh();
        }
#endif
    }
}
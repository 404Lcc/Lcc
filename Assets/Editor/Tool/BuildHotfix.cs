using Model;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildHotfix
{
    [InitializeOnLoadMethod]
    public static void InitBuildHotfix()
    {
#if !Release
        if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
        {
            File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Resources/Text/Unity.Hotfix.dll.bytes", true);
            File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Resources/Text/Unity.Hotfix.pdb.bytes", true);
            GameUtil.SaveAsset("Assets/Resources/Text/Unity.Hotfix.dll.bytes", GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", GameUtil.GetAsset("Assets/Resources/Text/Unity.Hotfix.dll.bytes")));
            Debug.Log("复制Hotfix.dll Hotfix.pdb");
            AssetDatabase.Refresh();
        }
#endif
    }
}
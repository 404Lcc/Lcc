using LccModel;
using System.IO;
using UnityEditor;
using UnityEngine;
using FileUtility = LccModel.FileUtility;

namespace LccEditor
{
    public class BuildHotfix
    {
        [InitializeOnLoadMethod]
        public static void Build()
        {
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
            {
                //if (EditorDefine.IsRelease)
                //{
                //    if (File.Exists("Unity.Hotfix.csproj"))
                //    {
                //        if (File.Exists("Assets/Res/DLL/Unity.Hotfix.dll.bytes"))
                //        {
                //            File.Delete("Assets/Res/DLL/Unity.Hotfix.dll.bytes");
                //        }
                //        if (File.Exists("Assets/Res/DLL/Unity.Hotfix.pdb.bytes"))
                //        {
                //            File.Delete("Assets/Res/DLL/Unity.Hotfix.pdb.bytes");
                //        }

                //        RoslynUtility.BuildDll("Unity.Hotfix.csproj", "Assets/Res/DLL/Unity.Hotfix.dll.bytes", BuildType.Release, false);
                //        FileUtil.SaveAsset("Assets/Res/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Res/DLL/Unity.Hotfix.dll.bytes")));
                //    }
                //}
                if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
                {
                    if (File.Exists("Assets/Res/DLL/Unity.Hotfix.dll.bytes"))
                    {
                        File.Delete("Assets/Res/DLL/Unity.Hotfix.dll.bytes");
                    }
                    if (File.Exists("Assets/Res/DLL/Unity.Hotfix.pdb.bytes"))
                    {
                        File.Delete("Assets/Res/DLL/Unity.Hotfix.pdb.bytes");
                    }

                    File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Res/DLL/Unity.Hotfix.dll.bytes", true);
                    File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Res/DLL/Unity.Hotfix.pdb.bytes", true);
                    //FileUtility.SaveAsset("Assets/Res/DLL/Unity.Hotfix.dll.bytes", RijndaelUtility.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtility.GetAsset("Assets/Res/DLL/Unity.Hotfix.dll.bytes")));
                }

                LinkUtility.BuildLink();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Hotfix丢失");
            }
        }
    }
}
using LccModel;
using System.IO;
using UnityEditor;
using UnityEngine;
using FileHelper = LccModel.FileHelper;

namespace LccEditor
{
    public class Build
    {
        [InitializeOnLoadMethod]
        public static void BuildHotfix()
        {
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
            {
                //if (EditorDefine.IsRelease)
                //{
                //    if (File.Exists("Unity.Hotfix.csproj"))
                //    {
                //        if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes"))
                //        {
                //            File.Delete("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes");
                //        }
                //        if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes"))
                //        {
                //            File.Delete("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes");
                //        }

                //        RoslynUtility.BuildDll("Unity.Hotfix.csproj", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", BuildType.Release, false);
                //        FileUtil.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
                //    }
                //}
                if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
                {
                    if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes"))
                    {
                        File.Delete("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes");
                    }
                    if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes"))
                    {
                        File.Delete("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes");
                    }

                    File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", true);
                    File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes", true);
                    FileHelper.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtility.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileHelper.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
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
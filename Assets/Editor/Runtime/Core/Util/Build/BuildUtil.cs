using LccModel;
using System.IO;
using UnityEditor;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class BuildUtil
    {
        [InitializeOnLoadMethod]
        public static void BuildHotfix()
        {
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
            {
                if (EditorDefine.IsRelease)
                {
                    if (File.Exists("Unity.Hotfix.csproj"))
                    {
                        if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes"))
                        {
                            File.Delete("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes");
                        }
                        if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes"))
                        {
                            File.Delete("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes");
                        }

                        RoslynUtil.BuildDll("Unity.Hotfix.csproj", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", BuildType.Release, false);
                        FileUtil.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
                    }
                }
                else
                {
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
                        FileUtil.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
                    }
                }

                LinkUtil.BuildLink();
                AssetDatabase.Refresh();
            }
            else
            {
                LogUtil.Debug("Hotfix丢失");
            }
        }
    }
}
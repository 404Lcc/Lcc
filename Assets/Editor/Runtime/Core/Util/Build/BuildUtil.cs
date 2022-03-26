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
                if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes"))
                {
                    File.Delete("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes");
                }
                if (File.Exists("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes"))
                {
                    File.Delete("Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes");
                }
#if Release
                if (File.Exists("Unity.Hotfix.csproj"))
                {
                    RoslynUtil.BuildDll("Unity.Hotfix.csproj", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", BuildType.Release, false);
                    FileUtil.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
                }
#else
                if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
                {
                    File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", true);
                    File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes", true);
                    FileUtil.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
                }
#endif
                LinkUtil.BuildLink();
                AssetDatabase.Refresh();
            }
            else
            {
                LogUtil.Log("Hotfix丢失");
            }
        }
    }
}
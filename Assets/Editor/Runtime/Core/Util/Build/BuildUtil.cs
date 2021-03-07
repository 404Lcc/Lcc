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
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef~"))
            {
                File.Move("Assets/Hotfix/Unity.Hotfix.asmdef~", "Assets/Hotfix/Unity.Hotfix.asmdef");
                LogUtil.Log("安装Hotfix");
                AssetDatabase.Refresh();
            }
            else
            {
                if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
                {
                    LogUtil.Log("安装Hotfix");
                }
                else
                {
                    LogUtil.Log("Hotfix丢失");
                }
            }
            if (File.Exists("Assets/Resources/DLL/Unity.Hotfix.dll.bytes"))
            {
                File.Delete("Assets/Resources/DLL/Unity.Hotfix.dll.bytes");
            }
            if (File.Exists("Assets/Resources/DLL/Unity.Hotfix.pdb.bytes"))
            {
                File.Delete("Assets/Resources/DLL/Unity.Hotfix.pdb.bytes");
            }
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
                RoslynUtil.BuildDll("Unity.Hotfix.csproj", "Assets/Resources/DLL/Unity.Hotfix.dll.bytes", BuildType.Release, false);
                FileUtil.SaveAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes")));
                File.Copy("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", true);
            }
#else
            if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
            {
                File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Resources/DLL/Unity.Hotfix.dll.bytes", true);
                File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Resources/DLL/Unity.Hotfix.pdb.bytes", true);
                FileUtil.SaveAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes")));
                File.Copy("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", true);
                File.Copy("Assets/Resources/DLL/Unity.Hotfix.pdb.bytes", "Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes", true);
            }
#endif
            LinkUtil.BuildLink();
            AssetDatabase.Refresh();
        }
    }
}
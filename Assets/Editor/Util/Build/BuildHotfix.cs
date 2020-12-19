using LccModel;
using System.IO;
using UnityEditor;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class BuildHotfix
    {
        [InitializeOnLoadMethod]
        public static void InitBuildHotfix()
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
#if !Release
            if (File.Exists("Library/ScriptAssemblies/Unity.Hotfix.dll") && File.Exists("Library/ScriptAssemblies/Unity.Hotfix.pdb"))
            {
                File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Resources/DLL/Unity.Hotfix.dll.bytes", true);
                File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Resources/DLL/Unity.Hotfix.pdb.bytes", true);
                File.Copy("Library/ScriptAssemblies/Unity.Hotfix.dll", "Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", true);
                File.Copy("Library/ScriptAssemblies/Unity.Hotfix.pdb", "Assets/Bundles/DLL/Unity.Hotfix.pdb.bytes", true);
                FileUtil.SaveAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes")));
                FileUtil.SaveAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes")));
                LogUtil.Log("复制Hotfix.dll Hotfix.pdb");
                AssetDatabase.Refresh();
            }
#endif
        }
    }
}
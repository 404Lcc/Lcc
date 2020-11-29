using LccModel;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class Preprocess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get; set;
        }
        public void OnPreprocessBuild(BuildReport report)
        {
#if Release
        if (File.Exists("Assets/Resources/DLL/Unity.Hotfix.dll.bytes"))
        {
            File.Delete("Assets/Resources/DLL/Unity.Hotfix.dll.bytes");
        }
        if (File.Exists("Assets/Resources/DLL/Unity.Hotfix.pdb.bytes"))
        {
            File.Delete("Assets/Resources/DLL/Unity.Hotfix.pdb.bytes");
        }
        if (File.Exists("Temp/bin/Release/Unity.Hotfix.dll"))
        {
            File.Copy("Temp/bin/Release/Unity.Hotfix.dll", "Assets/Resources/DLL/Unity.Hotfix.dll.bytes", true);
            FileUtil.SaveAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", RijndaelUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes")));
            AssetDatabase.Refresh();
        }
#endif
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
            {
                File.Move("Assets/Hotfix/Unity.Hotfix.asmdef", "Assets/Hotfix/Unity.Hotfix.asmdef~");
                LogUtil.Log("卸载Hotfix");
                AssetDatabase.Refresh();
            }
            else
            {
                if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef~"))
                {
                    LogUtil.Log("卸载Hotfix");
                }
                else
                {
                    LogUtil.Log("Hotfix丢失");
                }
            }
        }
        public void OnPostprocessBuild(BuildReport report)
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
        }
    }
}
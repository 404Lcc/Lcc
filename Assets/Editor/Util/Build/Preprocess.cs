using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

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
            GameUtil.SaveAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes", GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", GameUtil.GetAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes")));
            AssetDatabase.Refresh();
        }
#endif
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
            {
                File.Move("Assets/Hotfix/Unity.Hotfix.asmdef", "Assets/Hotfix/Unity.Hotfix.asmdef~");
                Debug.Log("卸载Hotfix");
                AssetDatabase.Refresh();
            }
            else
            {
                if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef~"))
                {
                    Debug.Log("卸载Hotfix");
                }
                else
                {
                    Debug.Log("Hotfix丢失");
                }
            }
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef~"))
            {
                File.Move("Assets/Hotfix/Unity.Hotfix.asmdef~", "Assets/Hotfix/Unity.Hotfix.asmdef");
                Debug.Log("安装Hotfix");
                AssetDatabase.Refresh();
            }
            else
            {
                if (File.Exists("Assets/Hotfix/Unity.Hotfix.asmdef"))
                {
                    Debug.Log("安装Hotfix");
                }
                else
                {
                    Debug.Log("Hotfix丢失");
                }
            }
        }
    }
}
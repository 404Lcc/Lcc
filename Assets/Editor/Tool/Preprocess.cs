using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class Preprocess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;
    public void OnPreprocessBuild(BuildReport report)
    {
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
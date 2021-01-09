using LccModel;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class PreprocessUtil : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get; set;
        }
        public void OnPreprocessBuild(BuildReport report)
        {
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
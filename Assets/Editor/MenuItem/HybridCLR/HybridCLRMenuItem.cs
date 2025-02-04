using System.IO;
using HybridCLR.Editor.Settings;
using LccModel;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public static class HybridCLRMenuItem
    {
        [MenuItem("HybridCLR/CopyAotDlls")]
        public static void CopyAotDll()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string fromDir = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
            string toDir = "Assets/Res/AotDlls";
            if (Directory.Exists(toDir))
            {
                Directory.Delete(toDir, true);
            }
            Directory.CreateDirectory(toDir);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < HybridCLRSettings.Instance.patchAOTAssemblies.Length; i++)
            {
                var aotDll = HybridCLRSettings.Instance.patchAOTAssemblies[i];

                if (i == HybridCLRSettings.Instance.patchAOTAssemblies.Length - 1)
                {
                    sb.Append(aotDll);
                }
                else
                {
                    sb.Append(aotDll + "|");
                }


                File.Copy(Path.Combine(fromDir, aotDll), Path.Combine(toDir, $"{aotDll}.bytes"), true);
            }


            FileUtility.SaveAsset("Assets/Res/AotDlls/aot.bytes", sb.ToString());
            Debug.Log($"CopyAotDll Finish!");

            AssetDatabase.Refresh();
        }
    }
}
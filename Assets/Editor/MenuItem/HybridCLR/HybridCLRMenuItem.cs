using System.IO;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public static class HybridCLRMenuItem
    {
        [MenuItem("HybridCLR/¿½±´AotDll")]
        public static void CopyAotDll()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string fromDir = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
            string toDir = "Assets/Bundles/AotDlls";
            if (Directory.Exists(toDir))
            {
                Directory.Delete(toDir, true);
            }
            Directory.CreateDirectory(toDir);

            foreach (string aotDll in HybridCLRSettings.Instance.patchAOTAssemblies)
            {
                File.Copy(Path.Combine(fromDir, aotDll), Path.Combine(toDir, $"{aotDll}.bytes"), true);
            }
            Debug.Log($"CopyAotDll Finish!");

            AssetDatabase.Refresh();
        }
    }
}
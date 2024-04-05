using LccModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LccEditor
{
#if UNITY_EDITOR
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        /// <summary>
        /// 在构建应用程序前处理
        /// 原理：在构建APP之前，搜索StreamingAssets目录下的所有资源文件，然后将这些文件信息写入内置清单，内置清单存储在Resources文件夹下。
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            string saveFilePath = "Assets/Resources/BuildinFileManifest.asset";
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            string folderPath = $"{Application.dataPath}/StreamingAssets/{StreamingAssetsDefine.RootFolderName}";
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (root.Exists == false)
            {
                Debug.LogWarning($"没有发现YooAsset内置目录 : {folderPath}");
                return;
            }

            var manifest = ScriptableObject.CreateInstance<BuildinFileManifest>();
            FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                    continue;
                if (fileInfo.Name.StartsWith("PackageManifest_"))
                    continue;

                BuildinFileManifest.Element element = new BuildinFileManifest.Element();
                element.PackageName = fileInfo.Directory.Name;
                element.FileCRC32 = YooAsset.Editor.EditorTools.GetFileCRC32(fileInfo.FullName);
                element.FileName = fileInfo.Name;
                manifest.BuildinFiles.Add(element);
            }

            if (Directory.Exists("Assets/Resources") == false)
                Directory.CreateDirectory("Assets/Resources");
            UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"一共{manifest.BuildinFiles.Count}个内置文件，内置资源清单保存成功 : {saveFilePath}");
        }
    }
#endif
}
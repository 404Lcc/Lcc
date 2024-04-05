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
        /// �ڹ���Ӧ�ó���ǰ����
        /// ԭ���ڹ���APP֮ǰ������StreamingAssetsĿ¼�µ�������Դ�ļ���Ȼ����Щ�ļ���Ϣд�������嵥�������嵥�洢��Resources�ļ����¡�
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
                Debug.LogWarning($"û�з���YooAsset����Ŀ¼ : {folderPath}");
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
            Debug.Log($"һ��{manifest.BuildinFiles.Count}�������ļ���������Դ�嵥����ɹ� : {saveFilePath}");
        }
    }
#endif
}
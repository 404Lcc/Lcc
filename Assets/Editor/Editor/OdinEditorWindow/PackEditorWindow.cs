using System;
using System.IO;
using LccModel;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using JsonUtility = LccModel.JsonUtility;

namespace LccEditor
{
    [MenuTree("打包", 3)]
    public class PackEditorWindow : AEditorWindowBase
    {
        [PropertySpace(10)] [HideLabel, DisplayAsString]
        public string info = "打包";

        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public string packageVersion = "1";


        public PackEditorWindow()
        {
        }

        public PackEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }

        [PropertySpace(10)]
        [LabelText("生成资源"), Button(ButtonSizes.Gigantic)]
        public void GenerateAssets()
        {
            GenAssets(buildTarget, packageVersion);
        }

        private void DeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Debug.Log($"删除文件夹: {dir}");
                Directory.Delete(dir, true);
            }
        }

        private void CleanDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Debug.Log($"清理文件夹: {dir}");
                Directory.Delete(dir, true);
            }

            Directory.CreateDirectory(dir);
        }

        private void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = false)
        {
            // 获取源目录信息
            var dir = new DirectoryInfo(sourceDir);

            // 检查源目录是否存在
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"源目录不存在或无法找到: {sourceDir}");
            }

            // 如果目标目录不存在，则创建它
            Directory.CreateDirectory(destinationDir);

            // 获取源目录中的文件并拷贝到新位置
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            // 递归拷贝子目录
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, overwrite);
            }
        }

        private void GenAssets(BuildTarget buildTarget, string packageVersion)
        {
            Debug.LogWarning($"[Pack] 开始生成资源 {buildTarget}");

            try
            {
                var allPackagesBuildOutputDir = $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}/Packages/{packageVersion}";
                CleanDir(allPackagesBuildOutputDir);

                foreach (var packageName in AssetConfig.BPackageList)
                {
                    var onePackageOutputDir = $"{AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()}/{buildTarget}/{packageName}/{packageVersion}";
                    DeleteDir(onePackageOutputDir);

                    BuildResult buildResult = default;

                    if (packageName.StartsWith("Raw"))
                    {
                        RawFileBuildParameters buildParameters = new RawFileBuildParameters();
                        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                        buildParameters.BuildPipeline = nameof(EBuildPipeline.RawFileBuildPipeline);
                        buildParameters.BuildBundleType = (int)EBuildBundleType.RawBundle;
                        buildParameters.BuildTarget = buildTarget;
                        buildParameters.PackageName = packageName;
                        buildParameters.PackageVersion = packageVersion;
                        buildParameters.VerifyBuildingResult = true;
                        buildParameters.FileNameStyle = EFileNameStyle.HashName;
                        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
                        buildParameters.BuildinFileCopyParams = "";
                        buildParameters.ClearBuildCacheFiles = false;
                        buildParameters.UseAssetDependencyDB = false;
                        buildParameters.EncryptionServices = (IEncryptionServices)Activator.CreateInstance(typeof(EncryptionNone));

                        Debug.LogWarning($"[Pack] 构建名 {packageName} \n {JsonUtility.ToJson(buildParameters)}");

                        buildResult = new RawFileBuildPipeline().Run(buildParameters, true);
                    }
                    else
                    {
                        ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
                        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                        buildParameters.BuildPipeline = nameof(EBuildPipeline.ScriptableBuildPipeline);
                        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
                        buildParameters.BuildTarget = buildTarget;
                        buildParameters.PackageName = packageName;
                        buildParameters.PackageVersion = packageVersion;
                        buildParameters.EnableSharePackRule = true;
                        buildParameters.VerifyBuildingResult = true;
                        buildParameters.FileNameStyle = EFileNameStyle.HashName;
                        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
                        buildParameters.BuildinFileCopyParams = "";
                        buildParameters.CompressOption = ECompressOption.LZ4;
                        buildParameters.ClearBuildCacheFiles = false;
                        buildParameters.UseAssetDependencyDB = false;
                        buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName(packageName);
                        buildParameters.EncryptionServices = (IEncryptionServices)Activator.CreateInstance(typeof(EncryptionNone));

                        Debug.LogWarning($"[Pack] 构建名 {packageName} \n {JsonUtility.ToJson(buildParameters)}");

                        buildResult = new ScriptableBuildPipeline().Run(buildParameters, true);
                    }

                    if (buildResult.Success)
                    {
                        Debug.LogWarning($"[Pack] 构建名 {packageName} 成功 {buildResult.OutputPackageDirectory}");
                    }
                    else
                    {
                        throw new Exception($"[Pack] 构建名 {packageName} 失败 {buildResult.ErrorInfo}");
                    }

                    CopyDirectory(buildResult.OutputPackageDirectory, allPackagesBuildOutputDir);
                }

                Debug.LogWarning($"[Pack] 所有资源构建成功 {allPackagesBuildOutputDir}");

                if (!Application.isBatchMode)
                {
                    EditorUtility.RevealInFinder(allPackagesBuildOutputDir);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Pack] 构建失败: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private string GetBuiltinShaderBundleName(string packageName)
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }
    }
}
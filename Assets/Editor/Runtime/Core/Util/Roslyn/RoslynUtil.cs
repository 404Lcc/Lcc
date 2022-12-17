using LccModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LccEditor
{
    public static class RoslynUtil
    {
        /// <summary>
        /// 生成Dll
        /// </summary>
        /// <param name="csprojName"></param>
        /// <param name="path"></param>
        /// <param name="buildType"></param>
        /// <param name="isUseDefine"></param>
        /// <returns></returns>
        public static bool BuildDll(string csprojName, string path, BuildType buildType, bool isUseDefine)
        {
            bool isDebug = buildType == BuildType.Debug ? true : false;
            //项目相关所有宏
            List<string> defineList = new List<string>();
            //项目相关所有dll
            List<string> dllFilePathList = new List<string>();
            //项目本身cs文件
            List<string> csFilePathList = ReadCSPROJ(csprojName, ref defineList, ref dllFilePathList);
            List<Microsoft.CodeAnalysis.SyntaxTree> csFileList = new List<Microsoft.CodeAnalysis.SyntaxTree>();
            List<MetadataReference> dllFileList = new List<MetadataReference>();
            CSharpParseOptions parseOptions;
            //宏是否开启
            if (isUseDefine)
            {
                parseOptions = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: defineList);
            }
            else
            {
                parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            }
            //增加dll文件
            foreach (string item in dllFilePathList)
            {
                PortableExecutableReference dll = MetadataReference.CreateFromFile(item);
                if (dll == null)
                {
                    continue;
                }
                dllFileList.Add(dll);
            }
            //增加cs文件
            foreach (string item in csFilePathList)
            {
                if (File.Exists(item))
                {
                    Microsoft.CodeAnalysis.SyntaxTree cs = CSharpSyntaxTree.ParseText(FileUtil.GetAsset(item).GetString(), parseOptions, item, Encoding.UTF8);
                    if (cs == null)
                    {
                        continue;
                    }
                    csFileList.Add(cs);
                }
            }
            //设置编译参数
            CSharpCompilationOptions compilationOptions;
            if (isDebug)
            {
                compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug, warningLevel: 4, allowUnsafe: true);
            }
            else
            {
                compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, warningLevel: 4, allowUnsafe: true);
            }
            string assemblyName = Path.GetFileNameWithoutExtension(path);
            //开始编译
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, csFileList, dllFileList, compilationOptions);
            EmitResult result;
            if (isDebug)
            {
                string pdbPath = path.Replace(".dll", ".pdb");
                EmitOptions emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb, pdbFilePath: pdbPath);
                using (MemoryStream dllStream = new MemoryStream())
                {
                    using (MemoryStream pdbStream = new MemoryStream())
                    {
                        result = compilation.Emit(dllStream, pdbStream, options: emitOptions);
                        FileUtil.SaveAsset(path, dllStream.GetBuffer());
                        FileUtil.SaveAsset(pdbPath, pdbStream.GetBuffer());
                    }
                }
            }
            else
            {
                result = compilation.Emit(path);
            }
            if (result.Success)
            {
                LogUtil.Debug("编译成功");
            }
            else
            {
                List<Diagnostic> failureList = (from diagnostic in result.Diagnostics where diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error select diagnostic).ToList();
                foreach (Diagnostic item in failureList)
                {
                    LogUtil.Debug(item.ToString());
                }
            }
            return result.Success;
        }
        /// <summary>
        /// 读取CSPROJ项目
        /// </summary>
        /// <param name="path"></param>
        /// <param name="defineList"></param>
        /// <param name="dllFilePathList"></param>
        /// <returns></returns>
        public static List<string> ReadCSPROJ(string path, ref List<string> defineList, ref List<string> dllFilePathList)
        {
            List<string> csFilePathList = new List<string>();
            List<string> csprojPathList = new List<string>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlNode xmlNode = null;
            foreach (XmlNode item in xmlDocument.ChildNodes)
            {
                if (item.Name == "Project")
                {
                    xmlNode = item;
                    break;
                }
            }
            foreach (XmlNode item in xmlNode.ChildNodes)
            {
                if (item.Name == "PropertyGroup")
                {
                    foreach (XmlNode childItem in item.ChildNodes)
                    {
                        if (childItem.Name == "DefineConstants")
                        {
                            //宏引用
                            string define = childItem.InnerText;
                            defineList.AddRange(define.Split(';'));
                        }
                    }
                }
                if (item.Name == "ItemGroup")
                {
                    foreach (XmlNode childItem in item.ChildNodes)
                    {
                        if (childItem.Name == "Reference")
                        {
                            //dll引用
                            string dllFilePath = childItem.FirstChild.InnerText.Replace("\\", "/");
                            dllFilePathList.Add(dllFilePath);
                        }
                        if (childItem.Name == "Compile")
                        {
                            //cs引用
                            string csFilePath = childItem.Attributes[0].Value.Replace("\\", "/");
                            csFilePathList.Add(csFilePath);
                        }
                        if (childItem.Name == "ProjectReference")
                        {
                            //工程引用
                            string csprojFilePath = childItem.Attributes[0].Value;
                            csprojPathList.Add(csprojFilePath);
                        }
                    }
                }
            }
            //遍历工程引用
            foreach (string item in csprojPathList)
            {
                if (item.ToLower().Contains("editor"))
                {
                    continue;
                }
                ReadCSPROJ(item, ref defineList, ref dllFilePathList);
                string dllFilePath = "Library/ScriptAssemblies/" + item.Replace(".csproj", ".dll");
                if (File.Exists(dllFilePath))
                {
                    dllFilePathList.Add(dllFilePath);
                }
            }
            defineList = defineList.Distinct().ToList();
            //移除相关宏
            for (int i = defineList.Count - 1; i >= 0; i--)
            {
                if (defineList[i].Contains("UNITY_EDITOR"))
                {
                    defineList.RemoveAt(i);
                }
            }
            dllFilePathList = dllFilePathList.Distinct().ToList();
            return csFilePathList;
        }
    }
}
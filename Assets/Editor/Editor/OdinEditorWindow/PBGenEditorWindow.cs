using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LccModel;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [MenuTree("PB生成工具", 8)]
    public class PBGenEditorWindow : AEditorWindowBase
    {
        private readonly char[] splitChars = { ' ', '\t' };
        private const string ProtoPath = "Message/proto";
        private const string ProtoCSPath = "Message/protoCS";

        [Title("Proto生成工具", TitleAlignment = TitleAlignments.Centered)] [LabelText("协议导出路径")] [DisplayAsString] [ShowInInspector]
        private string OutputPath = Application.dataPath + "/Hotfix/GameLogic/Message/";

        public PBGenEditorWindow()
        {
        }

        public PBGenEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }

        [PropertySpace(10)]
        [LabelText("生成协议"), Button(size: ButtonSizes.Gigantic, Name = "生成协议")]
        public void ProtoGen()
        {
            if (!Directory.Exists(ProtoPath))
            {
                return;
            }

            var list = Directory.GetFiles(ProtoPath);
            if (list.Length == 0)
            {
                return;
            }

            if (Directory.Exists(ProtoCSPath))
            {
                Directory.Delete(ProtoCSPath, true);
            }

            Directory.CreateDirectory(ProtoCSPath);

            foreach (var item in list)
            {
                if (Path.GetExtension(item) == ".proto")
                {
                    Proto2CS($"./{ProtoPath}/{Path.GetFileNameWithoutExtension(item)}.proto", ProtoCSPath);
                }
            }
        }

        [PropertySpace(10)]
        [LabelText("导入"), Button(size: ButtonSizes.Gigantic, Name = "导入")]
        public void Import()
        {
            DirectoryInfo protoCSDirectory = Directory.CreateDirectory(ProtoCSPath);
            DirectoryInfo outputDirectory = Directory.CreateDirectory(OutputPath);

            Dictionary<string, FileInfo> protoCSFileDict = new Dictionary<string, FileInfo>();
            Dictionary<string, FileInfo> outputFileDict = new Dictionary<string, FileInfo>();

            foreach (var item in protoCSDirectory.GetFiles("*.cs"))
            {
                protoCSFileDict.Add(item.Name, item);
            }

            foreach (var fileInfo in outputDirectory.GetFiles("*.cs"))
            {
                outputFileDict.Add(fileInfo.Name, fileInfo);
            }

            bool isChange = false;

            //删除output有,但是protoCS没有的
            foreach (var fileName in outputFileDict.Keys)
            {
                if (protoCSFileDict.ContainsKey(fileName))
                    continue;

                //删除
                outputFileDict[fileName].Delete();

                Debug.Log($"删除文件 {fileName}");

                isChange = true;
            }

            //更新文件
            foreach (var fileName in protoCSFileDict.Keys)
            {
                FileInfo protoCSFile = protoCSFileDict[fileName];

                //已有更新
                if (outputFileDict.TryGetValue(fileName, out var outputFile))
                {
                    string protoCSFileMd5 = MD5Utility.ComputeMD5(File.ReadAllBytes(protoCSFile.FullName));
                    string outputFileMd5 = MD5Utility.ComputeMD5(File.ReadAllBytes(outputFile.FullName));

                    if (protoCSFileMd5.Equals(outputFileMd5) || protoCSFileMd5.CompareTo(outputFileMd5) == 0)
                        continue;

                    protoCSFile.CopyTo(outputFile.FullName, true);

                    Debug.Log($"替换文件 {protoCSFile.Name}");

                    isChange = true;
                }
                else
                {
                    //新文件复制

                    protoCSFile.CopyTo(OutputPath + protoCSFile.Name);

                    Debug.Log($"新增文件 {protoCSFile.Name}");

                    isChange = true;
                }
            }

            if (isChange)
            {
                AssetDatabase.Refresh();
            }
        }

        [PropertySpace(10)]
        [LabelText("打开协议文件夹"), Button(size: ButtonSizes.Gigantic, Name = "打开协议文件夹")]
        public void OpenProtoDirectory()
        {
            if (!Directory.Exists(ProtoPath))
            {
                Directory.CreateDirectory(ProtoPath);
            }

            EditorUtility.OpenWithDefaultApp(ProtoPath);
        }

        #region 生成CS

        private void Proto2CS(string protoName, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string proto = protoName;
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");

            string s = File.ReadAllText(proto);

            StringBuilder sb = new StringBuilder();
            sb.Append("using ProtoBuf;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("\n");
            sb.Append("namespace LccHotfix\n");
            sb.Append("{\n");

            bool isMessageStart = false;
            bool isEnumStart = false;
            string className = string.Empty;

            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "" || newline.StartsWith("option") || newline.StartsWith("reserved"))
                {
                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"\t{newline}\n");
                }

                if (newline.StartsWith("message"))
                {
                    isMessageStart = true;
                    string messageName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];

                    sb.Append($"\t[ProtoContract]\n");
                    className = messageName.Replace("{", "");
                    sb.Append($"\tpublic partial class {className} : MessageObject");

                    sb.Append("\n");

                    if (newline.EndsWith("{"))
                    {
                        sb.Append("\t{\n");
                    }

                    continue;
                }

                if (newline.StartsWith("enum"))
                {
                    isEnumStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];

                    sb.Append($"\tpublic enum {msgName.Replace("{", "")} ");

                    sb.Append("\n");

                    if (newline.EndsWith("{"))
                    {
                        sb.Append("\t{\n");
                    }

                    continue;
                }

                if (isMessageStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isMessageStart = false;
                        sb.Append("\t}\n");
                        continue;
                    }

                    if (newline.StartsWith("//"))
                    {
                        sb.AppendLine(newline);
                        continue;
                    }

                    if (newline != "" && newline != "}")
                    {
                        if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, newline);
                        }
                        else
                        {
                            Members(sb, newline);
                        }
                    }
                }

                if (isEnumStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isEnumStart = false;
                        sb.Append("\t}\n");
                        continue;
                    }

                    if (newline.StartsWith("//"))
                    {
                        sb.Append($"\t{newline}\n");
                        continue;
                    }

                    if (newline != "" && newline != "}")
                    {
                        sb.AppendLine("\t\t" + ConvertToPascalCase(newline).Replace(';', ','));
                    }
                }
            }

            sb.Append("}");

            using (FileStream txt = new FileStream(csPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(txt))
                {
                    sw.Write(sb.ToString());
                }
            }
        }

        private void Members(StringBuilder sb, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[0];
                string name = ss[1];
                int n = int.Parse(ss[3]);
                string typeCs = ConvertType(type);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic {typeCs} {name}{";"}\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"{newline}\n {e}");
            }
        }

        private void Repeated(StringBuilder sb, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int n = int.Parse(ss[4]);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"{newline}\n {e}");
            }
        }

        private string ConvertType(string type)
        {
            string typeCs;
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }

        /// <summary>
        /// 跳过第0个部分,首字母大写，其余字母小写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ConvertToPascalCase(string input)
        {
            string[] parts = input.Split('_');
            if (parts.Length <= 1)
                return input;
            //跳过第0个部分,首字母大写，其余字母小写
            for (int i = 1; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
                }
            }

            // 合并所有部分
            return string.Join("", parts);
        }

        #endregion
    }
}
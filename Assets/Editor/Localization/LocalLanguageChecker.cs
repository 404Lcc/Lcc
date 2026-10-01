using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace KaEditor.Editor.Language
{
    public class LocalLanguageChecker : EditorWindow
    {
        private Vector2 scrollPosition;
        private string resultText = "";
        private string coloredResultText = ""; // 带颜色的富文本版本
        private float resultAreaHeight = 500f;

        [MenuItem("工具箱/多语言Key检查工具")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalLanguageChecker>("多语言Key检查工具");
            window.minSize = new Vector2(800, 600);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            GUILayout.Label("多语言Key检查工具", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("检查规则：\n" +
                                    "1. 动态比较所有语言文件夹\n" +
                                    "2. 只列出每种语言缺少的key\n" +
                                    "3. 路径：Assets/Content/Config/Localization/\n" +
                                    "4. 语言目录文件夹：Strings（注意路径）\n" +
                                    "5. 检查结果顺序与原文件一致", MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("开始检查", GUILayout.Height(40)))
            {
                CheckAllLanguages();
            }

            if (!string.IsNullOrEmpty(coloredResultText))
            {
                GUILayout.Space(20);
                GUILayout.Label("检查结果：", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("复制结果", GUILayout.Height(25), GUILayout.Width(100)))
                {
                    EditorGUIUtility.systemCopyBuffer = resultText;
                    Debug.Log("结果已复制到剪贴板");
                }

                if (GUILayout.Button("清空结果", GUILayout.Height(25), GUILayout.Width(100)))
                {
                    resultText = "";
                    coloredResultText = "";
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                float windowHeight = position.height;
                float headerHeight = 200f;
                resultAreaHeight = Mathf.Max(400f, windowHeight - headerHeight);

                int lineCount = resultText.Count(c => c == '\n') + 1;
                if (lineCount > 100)
                {
                    resultAreaHeight = Mathf.Min(500f, windowHeight - headerHeight);
                }

                GUILayout.Space(5);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500));

                // 使用支持富文本的样式
                GUIStyle richTextStyle = new GUIStyle(EditorStyles.textArea);
                richTextStyle.richText = true; // 启用富文本
                richTextStyle.wordWrap = true;

                // 设置更好的文本颜色对比
                richTextStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f); // 浅灰色文字，在深色背景上更清晰

                float textAreaMinHeight = lineCount * 16f;
                EditorGUILayout.TextArea(coloredResultText, richTextStyle,
                    GUILayout.MinHeight(textAreaMinHeight),
                    GUILayout.ExpandWidth(true));

                EditorGUILayout.EndScrollView();

                EditorGUILayout.LabelField($"文本行数: {lineCount}", EditorStyles.miniLabel);
            }
        }

        private void CheckAllLanguages()
        {
            resultText = "";
            coloredResultText = "";
            StringBuilder sb = new StringBuilder();

            string basePath = Application.dataPath;
            string localizationPath = $"{basePath}/Content/Config/Localization";

            sb.AppendLine("=== 多语言Key检查报告 ===");
            sb.AppendLine($"检查时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"检查路径: {localizationPath}");
            sb.AppendLine();

            // 检查主文件夹是否存在
            if (!Directory.Exists(localizationPath))
            {
                sb.AppendLine("=== 文件夹检查 ===");
                sb.AppendLine($"X 多语言文件夹不存在: {localizationPath}");
                resultText = sb.ToString();
                coloredResultText = AddColorToText(resultText);
                Debug.LogError("多语言文件夹缺失，无法继续检查！");
                return;
            }

            // 获取所有语言文件夹
            string[] languageFolders = Directory.GetDirectories(localizationPath)
                .Select(f => new DirectoryInfo(f).Name)
                .OrderBy(f => f) // 按名称排序
                .ToArray();

            if (languageFolders.Length == 0)
            {
                sb.AppendLine("=== 文件夹检查 ===");
                sb.AppendLine($"X 没有找到任何语言文件夹: {localizationPath}");
                resultText = sb.ToString();
                coloredResultText = AddColorToText(resultText);
                Debug.LogError("没有找到任何语言文件夹！");
                return;
            }

            sb.AppendLine($"=== 发现的语言文件夹 ({languageFolders.Length}个) ===");
            foreach (string folder in languageFolders)
            {
                sb.AppendLine($"  - {folder}");
            }

            sb.AppendLine();

            // 加载所有语言的文件
            Dictionary<string, Dictionary<string, LanguageFileData>> allLanguageData =
                new Dictionary<string, Dictionary<string, LanguageFileData>>();

            List<string> missingFolders = new List<string>();

            foreach (string languageFolder in languageFolders)
            {
                string languagePath = $"{localizationPath}/{languageFolder}/Strings";

                if (!Directory.Exists(languagePath))
                {
                    sb.AppendLine($"X 语言文件夹 {languageFolder} 不存在 Strings 子文件夹: {languagePath}");
                    missingFolders.Add(languageFolder);
                    continue;
                }

                var languageFiles = LoadJsonFiles(languagePath, languageFolder);
                allLanguageData[languageFolder] = languageFiles;
            }

            if (missingFolders.Count > 0)
            {
                sb.AppendLine("=== 缺失的Strings文件夹 ===");
                foreach (string folder in missingFolders)
                {
                    sb.AppendLine($"  - {folder}: 缺少Strings文件夹");
                }

                sb.AppendLine();
            }

            // 收集所有文件名
            var allFileNames = new HashSet<string>();
            foreach (var languageData in allLanguageData.Values)
            {
                foreach (var file in languageData.Keys)
                {
                    allFileNames.Add(file);
                }
            }

            sb.AppendLine("=== 文件统计 ===");
            sb.AppendLine($"总文件数: {allFileNames.Count}");
            foreach (var languageFolder in languageFolders)
            {
                if (allLanguageData.ContainsKey(languageFolder))
                {
                    sb.AppendLine($"  {languageFolder}: {allLanguageData[languageFolder].Count} 个文件");
                }
                else
                {
                    sb.AppendLine($"  {languageFolder}: 0 个文件 (缺少Strings文件夹)");
                }
            }

            sb.AppendLine();

            // 检查文件缺失情况
            Dictionary<string, List<string>> missingFilesByLanguage = new Dictionary<string, List<string>>();
            foreach (string languageFolder in languageFolders)
            {
                if (!allLanguageData.ContainsKey(languageFolder))
                    continue;

                missingFilesByLanguage[languageFolder] = new List<string>();
            }

            foreach (string fileName in allFileNames.OrderBy(f => f))
            {
                foreach (string languageFolder in languageFolders)
                {
                    if (!allLanguageData.ContainsKey(languageFolder))
                        continue;

                    if (!allLanguageData[languageFolder].ContainsKey(fileName))
                    {
                        missingFilesByLanguage[languageFolder].Add(fileName);
                    }
                }
            }

            // 列出每种语言缺失的文件
            bool hasMissingFiles = missingFilesByLanguage.Values.Any(files => files.Count > 0);
            if (hasMissingFiles)
            {
                sb.AppendLine("=== 缺失文件统计 ===");

                foreach (var kvp in missingFilesByLanguage.OrderBy(k => k.Key))
                {
                    string languageFolder = kvp.Key;
                    List<string> missingFiles = kvp.Value;

                    if (missingFiles.Count > 0)
                    {
                        sb.AppendLine($"  【{languageFolder}】 缺少以下 {missingFiles.Count} 个文件:");
                        foreach (string fileName in missingFiles.OrderBy(f => f))
                        {
                            sb.AppendLine($"    - {fileName}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"  【{languageFolder}】: ✓ 无缺失文件");
                    }
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("=== 文件完整性 ===");
                sb.AppendLine($"✓ 所有语言文件完整，无缺失文件");
                sb.AppendLine();
            }

            int problemCount = 0;
            int totalMissingKeys = 0;

            if (allFileNames.Count > 0)
            {
                sb.AppendLine("=== Key缺失检查 ===");
            }

            foreach (string fileName in allFileNames.OrderBy(f => f))
            {
                // 检查这个文件在哪些语言中存在
                Dictionary<string, LanguageFileData> fileDataByLanguage = new Dictionary<string, LanguageFileData>();
                List<string> languagesWithFile = new List<string>();

                foreach (string languageFolder in languageFolders)
                {
                    if (!allLanguageData.ContainsKey(languageFolder))
                        continue;

                    if (allLanguageData[languageFolder].ContainsKey(fileName))
                    {
                        languagesWithFile.Add(languageFolder);
                        fileDataByLanguage[languageFolder] = allLanguageData[languageFolder][fileName];
                    }
                }

                // 如果文件不在所有语言中都存在，跳过key检查
                if (languagesWithFile.Count < 2)
                {
                    continue; // 至少需要2个语言才能比较
                }

                // 检查每种语言缺少的key
                bool hasProblem = false;
                int fileMissingKeys = 0;

                // 收集所有语言的key
                HashSet<string> allKeys = new HashSet<string>();
                foreach (var fileData in fileDataByLanguage.Values)
                {
                    allKeys.UnionWith(fileData.Keys);
                }

                // 构建结果字符串
                StringBuilder fileResult = new StringBuilder();

                // 文件标题：显示文件名和各语言key数量
                fileResult.AppendLine($"\n---- {fileName} -------------");

                // 检查每个语言缺少的key
                foreach (string languageFolder in languagesWithFile.OrderBy(l => l))
                {
                    var thisLanguageData = fileDataByLanguage[languageFolder];

                    // 找出这个语言缺少的key（在其他语言中存在，但在这个语言中不存在）
                    var missingKeys = new List<string>();
                    foreach (var otherLanguage in languagesWithFile)
                    {
                        if (otherLanguage == languageFolder)
                            continue;

                        foreach (var key in fileDataByLanguage[otherLanguage].Keys)
                        {
                            if (!thisLanguageData.KeySet.Contains(key) && !missingKeys.Contains(key))
                            {
                                missingKeys.Add(key);
                            }
                        }
                    }

                    if (missingKeys.Count > 0)
                    {
                        hasProblem = true;
                        fileMissingKeys += missingKeys.Count;

                        fileResult.AppendLine($"   【{languageFolder}】: 有缺少的key ({missingKeys.Count}个)");

                        // 按照当前语言文件中key的顺序显示缺少的key
                        var missingKeysInOrder = thisLanguageData.GetMissingKeysInOrder(missingKeys, languagesWithFile, fileDataByLanguage);

                        foreach (string key in missingKeysInOrder)
                        {
                            fileResult.AppendLine($"    - {key}");
                        }
                    }
                    else
                    {
                        fileResult.AppendLine($"   【{languageFolder}】: 无缺少的key");
                    }
                }

                // 计算共同key（在所有存在这个文件的语言中都有的key）
                if (languagesWithFile.Count > 1)
                {
                    var firstLanguageData = fileDataByLanguage[languagesWithFile[0]];
                    var commonKeys = new HashSet<string>(firstLanguageData.Keys);
                    for (int i = 1; i < languagesWithFile.Count; i++)
                    {
                        commonKeys.IntersectWith(fileDataByLanguage[languagesWithFile[i]].Keys);
                    }

                    fileResult.AppendLine($"   共同key数量: {commonKeys.Count}");
                }

                if (hasProblem)
                {
                    sb.Append(fileResult.ToString());
                    problemCount++;
                    totalMissingKeys += fileMissingKeys;
                }
            }

            sb.AppendLine($"\n=== 检查完成 ===");
            if (problemCount == 0 && !hasMissingFiles)
            {
                sb.AppendLine("\u2713 所有文件Key完全一致！");
            }
            else
            {
                sb.AppendLine("！！！发现多语言配置异常，请策划同学检查！！！！！！！！");
                int totalMissingFiles = missingFilesByLanguage.Values.Sum(files => files.Count);

                if (totalMissingFiles > 0)
                {
                    sb.AppendLine($"发现 {totalMissingFiles} 个缺失文件");
                    foreach (var kvp in missingFilesByLanguage.OrderBy(k => k.Key))
                    {
                        if (kvp.Value.Count > 0)
                        {
                            sb.AppendLine($"    {kvp.Key}: {kvp.Value.Count} 个");
                        }
                    }
                }

                if (problemCount > 0)
                {
                    sb.AppendLine($"发现 {problemCount}/{allFileNames.Count} 个文件存在Key缺失问题");
                    sb.AppendLine($"总计缺少的key: {totalMissingKeys}");
                }
            }

            resultText = sb.ToString();
            coloredResultText = AddColorToText(resultText);
            Debug.Log("多语言检查完成！");
        }

        private Dictionary<string, LanguageFileData> LoadJsonFiles(string path, string languageName)
        {
            var result = new Dictionary<string, LanguageFileData>();

            if (!Directory.Exists(path))
            {
                Debug.LogWarning($"{languageName}文件夹不存在: {path}");
                return result;
            }

            foreach (string filePath in Directory.GetFiles(path, "*.json"))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                    string fileName = Path.GetFileName(filePath);

                    var fileData = ParseJsonToLanguageFileData(jsonContent);

                    if (fileData != null && fileData.KeyCount > 0)
                    {
                        result[fileName] = fileData;
                    }
                    else
                    {
                        Debug.LogWarning($"无法解析{languageName}文件: {fileName}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"加载{languageName}文件失败 {Path.GetFileName(filePath)}: {e.Message}");
                }
            }

            return result;
        }

        private LanguageFileData ParseJsonToLanguageFileData(string jsonContent)
        {
            var fileData = new LanguageFileData();

            try
            {
                jsonContent = jsonContent.Trim();

                if (jsonContent.StartsWith("{") && jsonContent.EndsWith("}"))
                {
                    jsonContent = jsonContent.Substring(1, jsonContent.Length - 2).Trim();
                }

                string pattern = @"\""(?<key>[^""\\]*(?:\\.[^""\\]*)*)\""\s*:\s*\""(?<value>[^""\\]*(?:\\.[^""\\]*)*)\""";

                var matches = Regex.Matches(jsonContent, pattern);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        string key = match.Groups["key"].Value;
                        string value = match.Groups["value"].Value;

                        key = UnescapeString(key);
                        value = UnescapeString(value);

                        if (!string.IsNullOrEmpty(key))
                        {
                            // 保持key在文件中的顺序
                            fileData.AddKeyValue(key, value);
                        }
                    }
                }

                if (fileData.KeyCount == 0)
                {
                    fileData = ParseJsonSimpleToLanguageFileData(jsonContent);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"正则解析失败，尝试简单解析: {ex.Message}");
                fileData = ParseJsonSimpleToLanguageFileData(jsonContent);
            }

            return fileData;
        }

        private LanguageFileData ParseJsonSimpleToLanguageFileData(string jsonContent)
        {
            var fileData = new LanguageFileData();

            try
            {
                int index = 0;
                bool inKey = false;
                bool inValue = false;
                bool escapeNext = false;
                StringBuilder currentKey = new StringBuilder();
                StringBuilder currentValue = new StringBuilder();

                while (index < jsonContent.Length)
                {
                    char c = jsonContent[index];

                    if (escapeNext)
                    {
                        if (inKey) currentKey.Append(c);
                        else if (inValue) currentValue.Append(c);
                        escapeNext = false;
                    }
                    else if (c == '\\')
                    {
                        escapeNext = true;
                        if (inKey) currentKey.Append(c);
                        else if (inValue) currentValue.Append(c);
                    }
                    else if (c == '"')
                    {
                        if (!inKey && !inValue && index + 1 < jsonContent.Length && jsonContent[index + 1] == ':')
                        {
                            inKey = true;
                        }
                        else if (inKey)
                        {
                            inKey = false;

                            index++;
                            while (index < jsonContent.Length && (jsonContent[index] == ':' || char.IsWhiteSpace(jsonContent[index])))
                            {
                                index++;
                            }

                            if (index < jsonContent.Length && jsonContent[index] == '"')
                            {
                                inValue = true;
                            }

                            index--;
                        }
                        else if (inValue)
                        {
                            inValue = false;

                            string key = currentKey.ToString().Trim();
                            string value = currentValue.ToString().Trim();

                            if (!string.IsNullOrEmpty(key))
                            {
                                // 保持key在文件中的顺序
                                fileData.AddKeyValue(key, value);
                            }

                            currentKey.Clear();
                            currentValue.Clear();

                            index++;
                            while (index < jsonContent.Length && (jsonContent[index] == ',' || char.IsWhiteSpace(jsonContent[index])))
                            {
                                index++;
                            }

                            index--;
                        }
                    }
                    else
                    {
                        if (inKey) currentKey.Append(c);
                        else if (inValue) currentValue.Append(c);
                    }

                    index++;
                }
            }
            catch
            {
            }

            return fileData;
        }

        private string UnescapeString(string str)
        {
            return str.Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\/", "/")
                .Replace("\\b", "\b")
                .Replace("\\f", "\f")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }

        /// <summary>
        /// 添加颜色到文本，生成富文本
        /// </summary>
        private string AddColorToText(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";

            StringBuilder coloredText = new StringBuilder();
            string[] lines = plainText.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // 主标题 - 蓝色加粗
                if (line.StartsWith("=== ") && line.EndsWith(" ==="))
                {
                    line = "<color=#4FC3F7><b>" + line + "</b></color>";
                }
                // 二级标题（如=== 文件夹检查 ===） - 浅蓝色
                else if (line.Contains("=== 文件夹检查 ===") ||
                         line.Contains("=== 缺失的Strings文件夹 ===") ||
                         line.Contains("=== 文件统计 ===") ||
                         line.Contains("=== 缺失文件统计 ===") ||
                         line.Contains("=== 文件完整性 ===") ||
                         line.Contains("=== Key缺失检查 ===") ||
                         line.Contains("发现的语言文件夹") ||
                         line.Contains("=== 多语言Key检查报告 ===") ||
                         line.Contains("=== 检查完成 ==="))
                {
                    line = "<color=#81D4FA>" + line + "</color>";
                }
                // 文件标题（---- 文件名 ----） - 绿色
                else if (line.StartsWith("----")) // && line.EndsWith(" -------------")s
                {
                    line = "<color=#4DD0E1>" + line + "</color>";
                }
                // 成功标志（✓） - 亮绿色
                else if (line.Contains("✓"))
                {
                    line = line.Replace("✓", "<color=#00FF00>✓</color>");
                }
                // 失败标志（X） - 红色
                else if (line.Contains("X"))
                {
                    line = line.Replace("X", "<color=#FF5252>X</color>");
                }
                // 警告标志（！！！） - 红色加粗
                else if (line.Contains("！！！"))
                {
                    line = "<color=#FF5252><b>" + line + "</b></color>";
                }
                // 语言文件夹标记（【en】等） - 橙色
                else if (line.Contains("【") && line.Contains("】") && line.Contains("有缺少的"))
                {
                    // 保留语言文件夹标记的颜色
                    int start = line.IndexOf("【");
                    int end = line.IndexOf("】", start);
                    if (start >= 0 && end > start)
                    {
                        string before = line.Substring(0, start);
                        string languageFolder = line.Substring(start, end - start + 1);
                        string after = line.Substring(end + 1);
                        line = before + "<color=#FFA726>" + languageFolder + after + "</color>";
                    }
                }
                else if (line.Contains("无缺少"))
                {
                    line = "<color=#09FF14>" + line + "</color>";
                }
                // 文件统计中的数字 - 粉色
                else if (line.Contains("个文件") || line.Contains("个)") || line.Contains("总计缺少的key:") || line.Contains("数量"))
                {
                    // 提取数字并着色
                    var matches = Regex.Matches(line, @"\d+");
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            line = line.Replace(match.Value, "<color=#F48FB1>" + match.Value + "</color>");
                        }
                    }
                }

                coloredText.Append(line);
                if (i < lines.Length - 1)
                {
                    coloredText.Append("\n");
                }
            }

            return coloredText.ToString();
        }

        /// <summary>
        /// 自定义类，用于保持key的顺序
        /// </summary>
        private class LanguageFileData
        {
            private List<string> keyOrder = new List<string>();
            private Dictionary<string, string> keyValueDict = new Dictionary<string, string>();

            public int KeyCount => keyOrder.Count;
            public IEnumerable<string> Keys => keyOrder;
            public HashSet<string> KeySet => new HashSet<string>(keyOrder);

            public void AddKeyValue(string key, string value)
            {
                if (!keyValueDict.ContainsKey(key))
                {
                    keyOrder.Add(key);
                }

                keyValueDict[key] = value;
            }

            public string GetValue(string key)
            {
                return keyValueDict.ContainsKey(key) ? keyValueDict[key] : null;
            }

            // 获取缺少的key，按照当前语言文件中key的顺序返回
            public List<string> GetMissingKeysInOrder(List<string> missingKeys, List<string> languagesWithFile,
                Dictionary<string, LanguageFileData> allFileData)
            {
                // 首先尝试按照英文文件的顺序
                if (languagesWithFile.Contains("en") && allFileData.ContainsKey("en"))
                {
                    return GetMissingKeysInReferenceOrder(missingKeys, allFileData["en"].keyOrder);
                }

                // 如果没有英文，尝试按照第一个参考语言的顺序
                foreach (var language in languagesWithFile)
                {
                    if (language != "en" && allFileData.ContainsKey(language))
                    {
                        return GetMissingKeysInReferenceOrder(missingKeys, allFileData[language].keyOrder);
                    }
                }

                // 最后按照当前语言的顺序
                return GetMissingKeysInReferenceOrder(missingKeys, this.keyOrder);
            }

            private List<string> GetMissingKeysInReferenceOrder(List<string> missingKeys, List<string> referenceOrder)
            {
                // 按照参考顺序对缺少的key进行排序
                var orderedMissingKeys = new List<string>();

                // 首先添加在参考顺序中找到的key
                foreach (var key in referenceOrder)
                {
                    if (missingKeys.Contains(key))
                    {
                        orderedMissingKeys.Add(key);
                    }
                }

                // 然后添加在参考顺序中没找到的key（如果有的话）
                foreach (var key in missingKeys)
                {
                    if (!orderedMissingKeys.Contains(key))
                    {
                        orderedMissingKeys.Add(key);
                    }
                }

                return orderedMissingKeys;
            }
        }
    }
}
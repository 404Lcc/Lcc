using LccModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public static class ExcelExportUtil
    {
        public const string ExcelPath = "Assets/Excels";
        public const string TemplatePath = "Assets/Editor/Runtime/Core/Template/ConfigTemplate.txt";

        public const string ModelClassPath = "Scripts/Runtime/Config/Config";
        public const string HotfixClassPath = "Hotfix/Runtime/Config/Config";

        public const string JsonPath = "Bundles/Config";

        public const string ProtobufPath = "Bundles/Config";

        public const string Hotfix = "Hotfix";

        private static string _content;
        public static string Content
        {
            get
            {
                if (string.IsNullOrEmpty(_content))
                {
                    _content = FileUtil.GetAsset(TemplatePath).Utf8ToStr();
                }
                return _content;
            }
        }
        public static void ExportScriptALL()
        {
            ExportScriptSelect(Directory.GetFiles(ExcelPath, "*.xlsx").ToList());
        }
        public static void ExportDataALL()
        {
            ExportDataSelect(Directory.GetFiles(ExcelPath, "*.xlsx").ToList());
        }

        public static void ExportScriptSelect(List<string> pathList)
        {
            float index = 0;
            foreach (string item in pathList)
            {
                index++;
                string name = Path.GetFileNameWithoutExtension(item);
                string suffix = Path.GetExtension(item);
                if (suffix != ".xlsx") return;
                if (name.StartsWith("~")) return;
                EditorUtility.DisplayProgressBar("导表结构", $"正在导出{name}表", index / pathList.Count);
                XSSFWorkbook sheets = new XSSFWorkbook(File.Open(item, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                List<Cell> cellList = GetCells(sheets);
                if (item.Contains(Hotfix))
                {
                    ExportClass(name, cellList, ConfigType.Hotfix);
                }
                else
                {
                    ExportClass(name, cellList, ConfigType.Model);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        public static void ExportDataSelect(List<string> pathList)
        {
            float index = 0;
            foreach (string item in pathList)
            {
                index++;
                string name = Path.GetFileNameWithoutExtension(item);
                string suffix = Path.GetExtension(item);
                if (suffix != ".xlsx") return;
                if (name.StartsWith("~")) return;
                EditorUtility.DisplayProgressBar("导表结构", $"正在导出{name}表", index / pathList.Count);
                XSSFWorkbook sheets = new XSSFWorkbook(File.Open(item, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                List<Cell> cellList = GetCells(sheets);
                ExportJson(sheets, name, cellList);
                if (item.Contains(Hotfix))
                {
                    ExportProtobuf(name, ConfigType.Hotfix);
                }
                else
                {
                    ExportProtobuf(name, ConfigType.Model);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        #region 导出Class
        private static void ExportClass(string name, List<Cell> cellList, ConfigType configType)
        {
            string exportPath = $"{PathUtil.GetDataPath(GetClassPath(configType))}/{name}.cs";
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < cellList.Count; i++)
            {
                Cell cell = cellList[i];
                if (cell.attribute.StartsWith("#"))
                {
                    continue;
                }
                stringBuilder.Append($"\t\t[ProtoMember({i + 1}, IsRequired = true)]\n");
                stringBuilder.Append($"\t\tpublic {cell.type} {cell.name} {{ get; set; }}");
                if (i != cellList.Count - 1)
                {
                    stringBuilder.Append("\n");
                }
            }
            string content = Content.Replace("(LccModel)", $"Lcc{configType}").Replace("(CustomConfig)", name).Replace("(Propertys)", stringBuilder.ToString());
            FileUtil.SaveAsset(exportPath, content);
        }
        #endregion



        #region 导出Json
        private static void ExportJson(XSSFWorkbook xssfWorkbook, string name, List<Cell> cellList)
        {
            string exportPath = $"{PathUtil.GetDataPath(GetJsonPath())}/{Path.GetFileNameWithoutExtension(name)}.txt";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\n\t\"list\" : [");
            for (int i = 0; i < xssfWorkbook.NumberOfSheets; i++)
            {
                ExportJson(xssfWorkbook.GetSheetAt(i), cellList, stringBuilder);
            }
            stringBuilder.Append("]\n}");
            FileUtil.SaveAsset(exportPath, stringBuilder.ToString());
        }
        private static void ExportJson(ISheet sheet, List<Cell> cellList, StringBuilder stringBuilder)
        {
            if (sheet.GetRow(1) == null) return;
            for (int i = 4; i <= sheet.LastRowNum; i++)
            {
                stringBuilder.Append("\n\t\t{");
                for (int j = 0; j < cellList.Count; j++)
                {
                    Cell cell = cellList[j];
                    if (cell.attribute.Contains("#"))
                    {
                        continue;
                    }
                    if (cell.name != "Id")
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Append($"\"{cell.name}\" : {Convert(cell.type, GetCell(sheet, i, j))}");
                }
                if (i == sheet.LastRowNum)
                {
                    stringBuilder.Append("}");
                }
                else
                {
                    stringBuilder.Append("},\n");
                }
            }
        }
        #endregion



        #region 导出Protobuf
        private static void ExportProtobuf(string name, ConfigType configType)
        {
            string exportPath = PathUtil.GetDataPath(GetProtobufPath());
            string jsonPath = PathUtil.GetDataPath(GetJsonPath());

            string json = FileUtil.GetAsset($"{jsonPath}/{name}.txt").Utf8ToStr();
            object obj;
            if (configType == ConfigType.Model)
            {
                obj = JsonUtil.ToObject(typeof(Manager).Assembly.GetType($"{typeof(Manager).Namespace}.{name}Category"), json);
            }
            else
            {
                obj = JsonUtil.ToObject(typeof(LccHotfix.Manager).Assembly.GetType($"{typeof(LccHotfix.Manager).Namespace}.{name}Category"), json);
            }
            FileUtil.SaveAsset($"{exportPath}/{name}Category.bytes", ProtobufUtil.Serialize(obj));
        }
        #endregion



        private static List<Cell> GetCells(XSSFWorkbook xssfWorkbook)
        {
            List<Cell> cellList = new List<Cell>();
            HashSet<string> list = new HashSet<string>();
            for (int i = 0; i < xssfWorkbook.NumberOfSheets; i++)
            {
                GetCells(xssfWorkbook.GetSheetAt(i), cellList, list);
            }
            return cellList;
        }
        private static void GetCells(ISheet sheet, List<Cell> cellList, HashSet<string> list)
        {
            if (sheet.GetRow(1) == null) return;
            for (int i = 0; i < sheet.GetRow(1).LastCellNum; i++)
            {
                string fieldName = GetCell(sheet, 1, i);
                if (string.IsNullOrEmpty(fieldName))
                {
                    continue;
                }
                if (!list.Add(fieldName))
                {
                    continue;
                }
                string fieldAttribute = GetCell(sheet, 0, i);
                string fieldType = GetCell(sheet, 2, i);
                string fieldDesc = GetCell(sheet, 3, i);
                cellList.Add(new Cell(fieldAttribute, fieldName, fieldType, fieldDesc));
            }
        }

        private static string GetClassPath(ConfigType configType)
        {
            if (configType == ConfigType.Model)
            {
                return ModelClassPath;
            }
            return HotfixClassPath;
        }
        private static string GetJsonPath()
        {
            return JsonPath;
        }
        private static string GetProtobufPath()
        {
            return ProtobufPath;
        }



        private static string GetCell(ISheet sheet, int row, int cell)
        {
            IRow iRow = sheet?.GetRow(row);
            if (iRow != null)
            {
                return GetCell(iRow, cell);
            }
            return string.Empty;
        }
        private static string GetCell(IRow row, int cell)
        {
            ICell iCell = row?.GetCell(cell);
            if (iCell != null)
            {
                return GetCell(iCell);
            }
            return string.Empty;
        }
        private static string GetCell(ICell cell)
        {
            if (cell != null)
            {
                if (cell.CellType == CellType.Numeric || (cell.CellType == CellType.Formula && cell.CachedFormulaResultType == CellType.Numeric))
                {
                    return cell.NumericCellValue.ToString();
                }
                else if (cell.CellType == CellType.String || (cell.CellType == CellType.Formula && cell.CachedFormulaResultType == CellType.String))
                {
                    return cell.StringCellValue.ToString();
                }
                else if (cell.CellType == CellType.Boolean || (cell.CellType == CellType.Formula && cell.CachedFormulaResultType == CellType.Boolean))
                {
                    return cell.BooleanCellValue.ToString();
                }
                else
                {
                    return cell.ToString();
                }
            }
            return string.Empty;
        }

        private static string Convert(string type, string value)
        {
            switch (type)
            {
                case "int[]":
                case "int32[]":
                case "long[]":
                    return string.IsNullOrEmpty(value) ? "[]" : $"[{value}]";
                case "string[]":
                    StringBuilder stringBuilder = new StringBuilder();
                    string[] all = value.Trim().Split(',');
                    for (int i = 0; i < all.Length; i++)
                    {
                        stringBuilder.Append($"\"{all[i]}\"");
                        if (i != all.Length - 1)
                        {
                            stringBuilder.Append(",");
                        }
                    }
                    return string.IsNullOrEmpty(value) ? "[]" : $"[{stringBuilder}]";
                case "int":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    return string.IsNullOrEmpty(value) ? "0" : value;
                case "string":
                    return $"\"{value}\"";
                default:
                    throw new Exception($"不支持此类型 : {type}");
            }
        }
    }
}
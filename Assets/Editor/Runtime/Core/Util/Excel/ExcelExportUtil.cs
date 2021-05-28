using LccModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public static class ExcelExportUtil
    {
        public const string excelPath = "Assets/Excels";
        public const string templatePath = "Assets/Editor/Runtime/Core/Template/ConfigTemplate.txt";

        public const string modelClassPath = "Scripts/Runtime/Config/Config";
        public const string hotfixClassPath = "Hotfix/Runtime/Config/Config";

        public const string modelJsonPath = "Resources/Config";
        public const string hotfixJsonPath = "Bundles/Config";

        public const string modelProtobufPath = "Resources/Config";
        public const string hotfixProtobufPath = "Bundles/Config";

        public static string content;
        public static void ExportClassAndJson()
        {
            content = FileUtil.GetAsset(templatePath).GetString();
            foreach (string item in Directory.GetFiles(excelPath, "*.xlsx"))
            {
                ExportExcelClass(new XSSFWorkbook(item), Path.GetFileNameWithoutExtension(item), ConfigType.Model);
                ExportExcelClass(new XSSFWorkbook(item), Path.GetFileNameWithoutExtension(item), ConfigType.Hotfix);
                ExportExcelJson(new XSSFWorkbook(item), Path.GetFileNameWithoutExtension(item), ConfigType.Model);
                ExportExcelJson(new XSSFWorkbook(item), Path.GetFileNameWithoutExtension(item), ConfigType.Hotfix);
            }
            AssetDatabase.Refresh();
        }
        public static void ExportProtobuf()
        {
            ExportExcelProtobuf(ConfigType.Model);
            ExportExcelProtobuf(ConfigType.Hotfix);
            AssetDatabase.Refresh();
        }
        #region 导出Class
        public static void ExportExcelClass(XSSFWorkbook xssfWorkbook, string name, ConfigType configType)
        {
            List<Cell> cellList = new List<Cell>();
            HashSet<string> uniqes = new HashSet<string>();
            for (int i = 0; i < xssfWorkbook.NumberOfSheets; i++)
            {
                ExportSheetClass(xssfWorkbook.GetSheetAt(i), cellList, uniqes);
            }
            ExportClass(name, cellList, configType);
        }
        public static void ExportSheetClass(ISheet sheet, List<Cell> cellList, HashSet<string> uniqes)
        {
            if (sheet.GetRow(1) == null) return;
            for (int i = 0; i < sheet.GetRow(1).LastCellNum; i++)
            {
                string fieldName = GetCell(sheet, 1, i);
                if (string.IsNullOrEmpty(fieldName))
                {
                    continue;
                }
                if (!uniqes.Add(fieldName))
                {
                    continue;
                }
                string fieldAttribute = GetCell(sheet, 0, i);
                string fieldType = GetCell(sheet, 2, i);
                string fieldDesc = GetCell(sheet, 3, i);
                cellList.Add(new Cell(fieldAttribute, fieldName, fieldType, fieldDesc));
            }
        }
        public static void ExportClass(string name, List<Cell> cellList, ConfigType configType)
        {
            string exportPath = $"{PathUtil.GetPath(PathType.DataPath, GetClassPath(configType))}/{name}.cs";
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
            string content = ExcelExportUtil.content.Replace("(LccModel)", $"Lcc{configType}").Replace("(CustomConfig)", name).Replace("(Propertys)", stringBuilder.ToString());
            FileUtil.SaveAsset(exportPath, content);
        }
        #endregion
        #region 导出Json
        public static void ExportExcelJson(XSSFWorkbook xssfWorkbook, string name, ConfigType configType)
        {
            string exportPath = $"{PathUtil.GetPath(PathType.DataPath, GetJsonPath(configType))}/{Path.GetFileNameWithoutExtension(name)}.txt";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\n\t\"list\" : [");
            for (int i = 0; i < xssfWorkbook.NumberOfSheets; i++)
            {
                ExportSheetJson(xssfWorkbook.GetSheetAt(i), stringBuilder);
            }
            stringBuilder.Append("]\n}");
            FileUtil.SaveAsset(exportPath, stringBuilder.ToString());
        }
        public static void ExportSheetJson(ISheet sheet, StringBuilder stringBuilder)
        {
            if (sheet.GetRow(1) == null) return;
            List<Cell> cellList = new List<Cell>();
            for (int i = 0; i < sheet.GetRow(1).LastCellNum; i++)
            {
                string fieldAttribute = GetCell(sheet, 0, i);
                if (fieldAttribute.Contains("#"))
                {
                    continue;
                }
                string fieldName = GetCell(sheet, 1, i);
                if (string.IsNullOrEmpty(fieldName))
                {
                    continue;
                }
                string fieldType = GetCell(sheet, 2, i);
                string fieldDesc = GetCell(sheet, 3, i);
                cellList.Add(new Cell(fieldAttribute, fieldName, fieldType, fieldDesc));
            }
            for (int i = 4; i <= sheet.LastRowNum; i++)
            {
                stringBuilder.Append("\n\t\t{");
                for (int j = 0; j < sheet.GetRow(1).LastCellNum; j++)
                {
                    Cell cell = cellList[j];
                    if (cell.attribute == null)
                    {
                        continue;
                    }
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
        public static string Convert(string type, string value)
        {
            switch (type)
            {
                case "int[]":
                case "int32[]":
                case "long[]":
                    return $"[{value}]";
                case "string[]":
                    return $"[{value}]";
                case "int":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    return value;
                case "string":
                    return $"\"{value}\"";
                default:
                    throw new Exception($"不支持此类型 : {type}");
            }
        }
        #endregion
        #region 导出Protobuf
        public static void ExportExcelProtobuf(ConfigType configType)
        {
            string exportPath = PathUtil.GetPath(PathType.DataPath, GetProtobufPath(configType));
            string classPath = PathUtil.GetPath(PathType.DataPath, GetClassPath(configType));
            string jsonPath = PathUtil.GetPath(PathType.DataPath, GetJsonPath(configType));
            List<string> protoNameList = new List<string>();
            foreach (string item in Directory.GetFiles(classPath, "*.cs"))
            {
                protoNameList.Add(Path.GetFileNameWithoutExtension(item));
            }
            foreach (string item in protoNameList)
            {
                string json = FileUtil.GetAsset($"{jsonPath}/{item}.txt").GetString();
                object obj;
                if (configType == ConfigType.Model)
                {
                    obj = JsonUtil.ToObject(typeof(Manager).Assembly.GetType($"{typeof(Manager).Namespace}.{item}Category"), json);
                }
                else
                {
                    obj = JsonUtil.ToObject(typeof(LccHotfix.Manager).Assembly.GetType($"{typeof(LccHotfix.Manager).Namespace}.{item}Category"), json);
                }
                FileUtil.SaveAsset($"{exportPath}/{item}Category.bytes", ProtobufUtil.Serialize(obj));
            }
        }
        #endregion
        public static string GetClassPath(ConfigType configType)
        {
            if (configType == ConfigType.Model)
            {
                return modelClassPath;
            }
            return hotfixClassPath;
        }
        public static string GetJsonPath(ConfigType configType)
        {
            if (configType == ConfigType.Model)
            {
                return modelJsonPath;
            }
            return hotfixJsonPath;
        }
        public static string GetProtobufPath(ConfigType configType)
        {
            if (configType == ConfigType.Model)
            {
                return modelProtobufPath;
            }
            return hotfixProtobufPath;
        }
        public static string GetCell(ISheet sheet, int row, int cell)
        {
            IRow iRow = sheet?.GetRow(row);
            if (iRow != null)
            {
                return GetCell(iRow, cell);
            }
            return string.Empty;
        }
        public static string GetCell(IRow row, int cell)
        {
            ICell iCell = row?.GetCell(cell);
            if (iCell != null)
            {
                return GetCell(iCell);
            }
            return string.Empty;
        }
        public static string GetCell(ICell cell)
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
    }
}
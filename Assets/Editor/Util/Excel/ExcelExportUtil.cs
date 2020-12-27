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
        public static void ExportAll()
        {
            ExportJson();
            ExportToModel();
            ExportToHotfix();
        }
        public static void ExportJson()
        {
            foreach (string item in Directory.GetFiles("Assets/Excels"))
            {
                if (Path.GetExtension(item) != ".xlsx") continue;
                XSSFWorkbook xssfWorkbook = new XSSFWorkbook(item);
                FileUtil.SaveAsset($"Assets/Resources/Config/{Path.GetFileNameWithoutExtension(item)}.txt", SheetToJson(xssfWorkbook.GetSheetAt(0)));
                FileUtil.SaveAsset($"Assets/Bundles/Config/{Path.GetFileNameWithoutExtension(item)}.txt", SheetToJson(xssfWorkbook.GetSheetAt(0)));
            }
            AssetDatabase.Refresh();
        }
        public static void ExportToModel()
        {
            foreach (string item in Directory.GetFiles("Assets/Excels"))
            {
                if (Path.GetExtension(item) != ".xlsx") continue;
                FileUtil.SaveAsset($"Assets/Scripts/Runtime/Config/ConfigTable/{Path.GetFileNameWithoutExtension(item)}Table.cs", SheetToAConfigTable("namespace LccModel\n", Path.GetFileNameWithoutExtension(item)));
                XSSFWorkbook xssfWorkbook = new XSSFWorkbook(item);
                FileUtil.SaveAsset($"Assets/Scripts/Runtime/Config/Config/{Path.GetFileNameWithoutExtension(item)}.cs", SheetToConfig(xssfWorkbook.GetSheetAt(0), "namespace LccModel\n", Path.GetFileNameWithoutExtension(item)));
            }
            AssetDatabase.Refresh();
        }
        public static void ExportToHotfix()
        {
            foreach (string item in Directory.GetFiles("Assets/Excels"))
            {
                if (Path.GetExtension(item) != ".xlsx") continue;
                FileUtil.SaveAsset($"Assets/Hotfix/Runtime/Config/ConfigTable/{Path.GetFileNameWithoutExtension(item)}Table.cs", SheetToAConfigTable("using LccModel;\n\nnamespace LccHotfix\n", Path.GetFileNameWithoutExtension(item)));
                XSSFWorkbook xssfWorkbook = new XSSFWorkbook(item);
                FileUtil.SaveAsset($"Assets/Hotfix/Runtime/Config/Config/{Path.GetFileNameWithoutExtension(item)}.cs", SheetToConfig(xssfWorkbook.GetSheetAt(0), "namespace LccHotfix\n", Path.GetFileNameWithoutExtension(item)));
            }
            AssetDatabase.Refresh();
        }
        public static string SheetToJson(ISheet sheet)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\n\t");
            //第0行格子个数
            int count = sheet.GetRow(0).LastCellNum;
            List<Cell> cellList = new List<Cell>();
            for (int i = 0; i < count; i++)
            {
                string fieldName = GetCell(sheet, 0, i);
                string fieldType = GetCell(sheet, 1, i);
                string fieldDesc = GetCell(sheet, 2, i);
                cellList.Add(new Cell(fieldName, fieldType, fieldDesc));
            }
            //从第三行开始到最后
            for (int i = 3; i <= sheet.LastRowNum; i++)
            {
                if (string.IsNullOrEmpty(GetCell(sheet, i, 0))) continue;
                IRow iRow = sheet.GetRow(i);
                for (int j = 0; j < count; j++)
                {
                    Cell cell = cellList[j];
                    if (cell.desc.StartsWith("#")) continue;
                    string value = GetCell(iRow, j);
                    if (string.IsNullOrEmpty(value)) continue;
                    if (j > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    if (cell.name == "id")
                    {
                        stringBuilder.Append($"\"{value}\" : {{");
                    }
                    stringBuilder.Append($"\"{cell.name}\" : {Convert(cell.type, value)}");
                }
                if (i < sheet.LastRowNum)
                {
                    stringBuilder.Append("},\n\t");
                }
                else
                {
                    stringBuilder.Append("}\n");
                }
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
        public static string SheetToAConfigTable(string classHead, string configName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(classHead);
            stringBuilder.Append("{\n");
            stringBuilder.Append("\t[Config]\n");
            stringBuilder.Append($"\tpublic class {configName}Table : AConfigTable<{configName}>\n");
            stringBuilder.Append("\t{\n");
            stringBuilder.Append("\t}\n");
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
        public static string SheetToConfig(ISheet sheet, string classHead, string configName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(classHead);
            stringBuilder.Append("{\n");
            stringBuilder.Append($"\tpublic class {configName} : IConfig\n");
            stringBuilder.Append("\t{\n");
            stringBuilder.Append("\t\tpublic int Id\n");
            stringBuilder.Append("\t\t{\n");
            stringBuilder.Append("\t\t\tget; set;\n");
            stringBuilder.Append("\t\t}");
            //第0行格子个数
            int count = sheet.GetRow(0).LastCellNum;
            for (int i = 0; i < count; i++)
            {
                string fieldName = GetCell(sheet, 0, i);
                string fieldType = GetCell(sheet, 1, i);
                string fieldDesc = GetCell(sheet, 2, i);
                if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(fieldType) || fieldDesc.StartsWith("#")) continue;
                if (fieldName == "id") continue;
                if (i > 0)
                {
                    stringBuilder.Append("\n");
                }
                stringBuilder.Append($"\t\tpublic {fieldType} {fieldName};\n");
            }
            stringBuilder.Append("\t}\n");
            stringBuilder.Append("}");
            return stringBuilder.ToString();
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
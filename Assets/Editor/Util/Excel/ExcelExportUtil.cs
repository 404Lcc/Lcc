using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

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
                using (FileStream fileStream = new FileStream($"Assets/Resources/Config/{Path.GetFileNameWithoutExtension(item)}.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(SheetToJson(xssfWorkbook.GetSheetAt(0)));
                    }
                }
            }
            AssetDatabase.Refresh();
        }
        public static void ExportToModel()
        {
            foreach (string item in Directory.GetFiles("Assets/Excels"))
            {
                if (Path.GetExtension(item) != ".xlsx") continue;
                using (FileStream fileStream = new FileStream($"Assets/Scripts/Runtime/Config/ConfigTable/{Path.GetFileNameWithoutExtension(item)}Table.cs", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(SheetToAConfigTable("namespace LccModel\n", $"{Path.GetFileNameWithoutExtension(item)}"));
                    }
                }
                XSSFWorkbook xssfWorkbook = new XSSFWorkbook(item);
                using (FileStream fileStream = new FileStream($"Assets/Scripts/Runtime/Config/Config/{Path.GetFileNameWithoutExtension(item)}.cs", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(SheetToConfig(xssfWorkbook.GetSheetAt(0), "namespace LccModel\n", $"{Path.GetFileNameWithoutExtension(item)}"));
                    }
                }
            }
            AssetDatabase.Refresh();
        }
        public static void ExportToHotfix()
        {
            foreach (string item in Directory.GetFiles("Assets/Excels"))
            {
                if (Path.GetExtension(item) != ".xlsx") continue;
                using (FileStream fileStream = new FileStream($"Assets/Hotfix/Runtime/Config/ConfigTable/{Path.GetFileNameWithoutExtension(item)}Table.cs", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(SheetToAConfigTable("using LccModel;\n\nnamespace LccHotfix\n", $"{Path.GetFileNameWithoutExtension(item)}"));
                    }
                }
                XSSFWorkbook xssfWorkbook = new XSSFWorkbook(item);
                using (FileStream fileStream = new FileStream($"Assets/Hotfix/Runtime/Config/Config/{Path.GetFileNameWithoutExtension(item)}.cs", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(SheetToConfig(xssfWorkbook.GetSheetAt(0), "namespace LccHotfix\n", $"{Path.GetFileNameWithoutExtension(item)}"));
                    }
                }
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
            return sheet?.GetRow(row)?.GetCell(cell)?.ToString() ?? string.Empty;
        }
        public static string GetCell(IRow row, int cell)
        {
            return row?.GetCell(cell)?.ToString() ?? string.Empty;
        }
    }
}
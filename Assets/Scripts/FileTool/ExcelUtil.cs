using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MiniExcelLibs;
using System.Linq;
using System.IO;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text.RegularExpressions;
using Color = System.Drawing.Color;
using MiniExcelLibs.OpenXml; // 确保引用正确的命名空间
public static class ExcelUtil
{
    public static string extension = ".xlsx";
    public static string ExcelPath
    {
        get
        {
            //Debug.LogError($"GDataManager.Instance.ExcelPath:{GDataManager.Instance.ExcelPath}");
            return $"{Application.dataPath}/Excel";
        }
    }

    public static List<T> GetListOject<T>(string path, string excelName, string sheetName, string startCell = "A1") where T : class, new()
    {
        string path_full = $"{path}/{excelName}{extension}";

        return MiniExcel.Query<T>(path_full, sheetName: sheetName, excelType: ExcelType.XLSX, startCell: startCell)
        //.Where(row=>!row.IsDefault(true))//只取有效行
        .ToList();

    }

    public static Dictionary<TKey, T> GetDictionary<TKey, T>(string path, string excelName, string sheetName, string startCell = "A1", string keyName = "Key") where T : class, new()
    {
        List<T> listT = GetListOject<T>(path, excelName, sheetName, startCell);
        Dictionary<TKey, T> dic = null;
        if (listT != null && listT.Count > 0)
        {
            dic = new Dictionary<TKey, T>(listT.Count);
            object key;
            for (int i = 0; i < listT.Count; ++i)
            {
                var value = listT[i];
                if (value == null) continue;
                key = value.GetPropertyValue(keyName);
                dic[(TKey)key] = value;
            }
        }

        if (dic == null)
        {
            dic = new Dictionary<TKey, T>();
        }

        return dic;
    }

    public static string SelectSaveExcleFile(string fileName, string title, string suffix = "*.xlsx")
    {
        string namePointer = string.Copy(fileName);
        string tempPath = Common.FileDialog.OpenFileDialogToSave(namePointer, $"Excel表格({suffix})\0{suffix}\0", title, "");

        return tempPath;
    }

    public static void SaveExcel(string filePath, Dictionary<string, string> dictionary)
    {
        // 转换字典为适合 MiniExcel 的格式
        var data = new List<Dictionary<string, object>>();
        foreach (var key in dictionary.Keys)
        {
            var row = new Dictionary<string, object>
            {
                { "Key", key },
                { "Value", dictionary[key] }
            };
            data.Add(row);
        }

        try
        {
            // 使用 MiniExcel 保存数据到 Excel 文件
            MiniExcel.SaveAs(filePath, data, overwriteFile: true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"保存数据时出错: {ex.Message}");
        }
    }

    public static void SaveExcel<D, T>(string filePath, string sheetName, Dictionary<D, T> dictionary)
    {
        // 转换字典为适合 MiniExcel 的格式
        var data = new List<Dictionary<string, object>>();
        foreach (var kvp in dictionary)
        {
            var row = new Dictionary<string, object>();

            // 获取类的属性并添加到行数据中
            PropertyInfo[] properties = kvp.Value.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var ignoreAttribute = property.GetCustomAttribute<MiniExcelLibs.Attributes.ExcelIgnoreAttribute>();
                if (ignoreAttribute == null)
                {
                    row[property.Name] = property.GetValue(kvp.Value);
                }
            }

            data.Add(row);
        }

        try
        {
            // 使用 MiniExcel 保存数据到 Excel 文件
            MiniExcel.SaveAs(filePath, data, sheetName: sheetName, overwriteFile: true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"保存数据时出错: {ex.Message}");
        }
    }
    public static void SaveExcelMultiLine<D, T>(string filePath, string sheetName, Dictionary<D, T> dictionary)
    {
        // 设置 EPPlus 许可证
        //OfficeOpenXml.EPPlusLicense.SetNonCommercialPersonal(System.String)
        ExcelPackage.License.SetNonCommercialPersonal("samo"); // 非商业用途
        // 转换字典为适合 MiniExcel 的格式
        var data = new List<Dictionary<string, object>>();
        foreach (var kvp in dictionary)
        {
            var row = new Dictionary<string, object>();

            // 获取类的属性并添加到行数据中
            PropertyInfo[] properties = kvp.Value.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var ignoreAttribute = property.GetCustomAttribute<MiniExcelLibs.Attributes.ExcelIgnoreAttribute>();
                if (ignoreAttribute == null)
                {
                    // 获取属性值
                    var value = property.GetValue(kvp.Value);

                    // 处理字符串中的换行符
                    if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                    {
                        // 将普通换行符替换为Excel可识别的换行符
                        stringValue = stringValue.Replace("\n", "\u000A");
                        row[property.Name] = stringValue;
                    }
                    else
                    {
                        row[property.Name] = value;
                    }
                }
            }

            data.Add(row);
        }

        try
        {
            // 使用 MiniExcel 保存数据到 Excel 文件
            MiniExcel.SaveAs(filePath, data, sheetName: sheetName, overwriteFile: true);

            // 打开文件并设置自动换行（需要EPPlus库）
            using var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet != null)
            {
                // 设置所有单元格自动换行
                worksheet.Cells.AutoFitColumns(5, 15);
                worksheet.Cells.Style.WrapText = true;
                package.Save();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"保存数据时出错: {ex.Message}");
        }
    }


    public static void SaveExcelWithHtmlFormatting<D, T>(string filePath, string sheetName, Dictionary<D, T> dictionary)
    {
        // 设置许可证
        ExcelPackage.License.SetNonCommercialPersonal("samo");

        try
        {
            // 确保目录存在
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"创建目录: {directory}");
            }

            // 检查文件是否已存在
            bool fileExists = File.Exists(filePath);
            FileInfo fileInfo = new FileInfo(filePath);
            
            // 检查文件是否被锁定
            if (fileExists && IsFileLocked(fileInfo))
            {
                Debug.LogError($"无法保存文件：请先关闭已打开的Excel文件 '{fileInfo.Name}'");
                return;
            }

            using var package = fileExists
                ? new ExcelPackage(fileInfo)
                : new ExcelPackage(); // 新文件

            // 关键修改：处理工作表重名
            ExcelWorksheet worksheet;
            if (package.Workbook.Worksheets.Any(w => w.Name == sheetName))
            {
                // 删除已存在的工作表
                package.Workbook.Worksheets.Delete(sheetName);
                worksheet = package.Workbook.Worksheets.Add(sheetName);
            }
            else
            {
                // 不存在则直接创建
                worksheet = package.Workbook.Worksheets.Add(sheetName);
            }

            // 添加表头和数据（保持原有逻辑不变）
            var properties = typeof(T).GetProperties();
            for (int col = 0; col < properties.Length; col++)
            {
                var cell = worksheet.Cells[1, col + 1];
                cell.Value = properties[col].Name;
                ApplyCellStyle(cell, "font-weight:bold; background-color:#D3D3D3; text-align:center");
            }

            int row = 2;
            foreach (var item in dictionary.Values)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var cell = worksheet.Cells[row, col + 1];
                    var value = properties[col].GetValue(item)?.ToString() ?? "";

                    if (value.Contains("[") && value.Contains("]"))
                    {
                        ParseFormattedText(cell, value);
                    }
                    else
                    {
                        cell.Value = value;
                    }

                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.WrapText = true;

                    // 设置背景颜色（关键代码）
                    if (value == string.Empty)
                    {
                        cell.Style.Fill.SetBackground(Color.LightGray, ExcelFillStyle.Solid);
                    }
                    if (value == WorkShiftTime.OffShiftName)
                    {
                        cell.Style.Fill.SetBackground(Color.IndianRed, ExcelFillStyle.Solid);
                    }
                }
                row++;

            }

            // 设置边框样式（细边框，黑色）
            //SetBorder(worksheet.Cells, ExcelBorderStyle.Thin, Color.Black);

            // 调整列宽
            for (int col = 1; col <= properties.Length; col++)
            {
                worksheet.Column(col).AutoFit();
            }

            // 明确保存到指定路径（对于新文件）
            if (!fileExists)
            {
                package.SaveAs(fileInfo);
            }
            else
            {
                package.Save();
            }
            
            Debug.Log($"文件保存成功: {filePath}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Debug.LogError($"保存文件失败：目录不存在 - {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.LogError($"保存文件失败：没有权限 - {ex.Message}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"保存文件失败：IO错误 - {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"保存数据时出错: {ex.Message}");
        }
    }

    private static void SetBorder(ExcelRange range, ExcelBorderStyle style, Color color)
    {
        // 设置四周边框
        range.Style.Border.Top.Style = style;
        range.Style.Border.Bottom.Style = style;
        range.Style.Border.Left.Style = style;
        range.Style.Border.Right.Style = style;

        // 设置边框颜色
        range.Style.Border.Top.Color.SetColor(color);
        range.Style.Border.Bottom.Color.SetColor(color);
        range.Style.Border.Left.Color.SetColor(color);
        range.Style.Border.Right.Color.SetColor(color);
 
    }

    // 改进的文件锁定检查方法
    private static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using (FileStream stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            return true;
        }

        return false;
    }
    // 解析带格式的文本并应用到单元格
    private static void ParseFormattedText(ExcelRange cell, string formattedText)
    {
        cell.Value = null; // 清空单元格原有内容
        cell.Style.WrapText = true; // 启用自动换行
        
        // 关键：不设置单元格默认颜色，让富文本各自控制颜色

        // 按换行符拆分文本
        var lines = formattedText.Split(new[] { '\n' }, StringSplitOptions.None);

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            int lastPosition = 0;

            // 匹配颜色标签（使用非贪婪模式，确保精确匹配）
            var colorMatches = Regex.Matches(line, @"\[color=([\#a-zA-Z0-9]+)\](.*?)\[/color\]", RegexOptions.Singleline);

            foreach (Match match in colorMatches)
            {
                // 1. 添加标签前的普通文本（强制黑色）
                if (match.Index > lastPosition)
                {
                    string plainText = line.Substring(lastPosition, match.Index - lastPosition);
                    var plainRichText = cell.RichText.Add(plainText);
                    ParseInnerTags(plainRichText, plainText);
                    plainRichText.Color = Color.Black; // 显式设置为黑色，不继承任何颜色
                }

                // 2. 添加带颜色的文本
                string colorCode = match.Groups[1].Value;
                string coloredText = match.Groups[2].Value;
                Color color = CustomColorTranslator.FromHtml(colorCode);

                var coloredRichText = cell.RichText.Add(coloredText);
                coloredRichText.Color = color; // 仅当前片段使用指定颜色
                ParseInnerTags(coloredRichText, coloredText);

                lastPosition = match.Index + match.Length;
            }

            // 3. 添加行尾剩余的普通文本（强制黑色）
            if (lastPosition < line.Length)
            {
                string remainingText = line.Substring(lastPosition);
                var remainingRichText = cell.RichText.Add(remainingText);
                ParseInnerTags(remainingRichText, remainingText);
                remainingRichText.Color = Color.Black; // 显式设置为黑色
            }

            // 4. 添加换行符（最后一行除外）
            if (lineIndex != lines.Length - 1)
            {
                var newlineText = cell.RichText.Add("\n");
                newlineText.Color = Color.Black; // 换行符不影响颜色
            }
        }
    }

    // 处理内部标签（保持不变）
    private static void ParseInnerTags(ExcelRichText richText, string text)
    {
        // 处理粗体
        if (text.Contains("[b]") && text.Contains("[/b]"))
        {
            richText.Bold = true;
            richText.Text = richText.Text.Replace("[b]", "").Replace("[/b]", "");
        }

        // 处理斜体
        if (text.Contains("[i]") && text.Contains("[/i]"))
        {
            richText.Italic = true;
            richText.Text = richText.Text.Replace("[i]", "").Replace("[/i]", "");
        }
    }
    
    // 添加格式化文本（处理换行和内部标签）
    private static void AddFormattedText(ExcelRange cell, string text)
    {
        // 处理换行符
        var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                // 添加换行（使用Excel的换行符）
                cell.RichText.Add("\n");
            }

            // 处理当前行的格式标签
            var richText = cell.RichText.Add(lines[i]);
            ParseInnerTags(richText, lines[i]);
        }
    }

 
    // 应用单元格样式（用于表头）
    private static void ApplyCellStyle(ExcelRange cell, string styleString)
    {
        var styles = styleString.Split(';')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => s.Split(':'))
            .ToDictionary(
                parts => parts[0].Trim(),
                parts => parts.Length > 1 ? parts[1].Trim() : ""
            );

        foreach (var style in styles)
        {
            switch (style.Key.ToLower())
            {
                case "font-weight":
                    cell.Style.Font.Bold = style.Value.ToLower() == "bold";
                    break;
                case "color":
                    Color color = CustomColorTranslator.FromHtml(style.Value);
                    cell.Style.Font.Color.SetColor(color);

                    break;
                case "background-color":
                    Color bgColor = CustomColorTranslator.FromHtml(style.Value);

                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(bgColor);

                    break;
                case "text-align":
                    switch (style.Value.ToLower())
                    {
                        case "center":
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case "right":
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        case "left":
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                    }
                    break;
            }
        }
    }

    public static class CustomColorTranslator
    {
        // 解析 HTML 颜色代码（#RRGGBB 或 #RGB 格式）
        public static Color FromHtml(string htmlColor)
        {
            if (string.IsNullOrWhiteSpace(htmlColor))
                return Color.Black;

            // 处理 # 开头的颜色（如 #FF0000 或 #F00）
            if (htmlColor.StartsWith("#"))
            {
                string hex = htmlColor.Substring(1).Trim();

                // 处理 3 位缩写（如 #F00 → #FF0000）
                if (hex.Length == 3)
                {
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
                }

                // 解析 6 位十六进制颜色
                if (hex.Length == 6 &&
                    int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int colorValue))
                {
                    return Color.FromArgb(
                        (colorValue >> 16) & 0xFF,  // 红色
                        (colorValue >> 8) & 0xFF,   // 绿色
                        colorValue & 0xFF           // 蓝色
                    );
                }
            }

            // 处理颜色名称（如 "Red"、"Blue"）
            try
            {
                // 尝试通过颜色名称获取
                return Color.FromName(htmlColor);
            }
            catch
            {
                // 名称无效时返回默认颜色
                return Color.Black;
            }
        }
    }

}

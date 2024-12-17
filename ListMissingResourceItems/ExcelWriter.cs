using ClosedXML.Excel;
using System.Globalization;

public class ExcelWriter
{
    public void Write(Dictionary<string, string?> mainFile, Dictionary<CultureInfo, Dictionary<string, string>> result, string excelFilePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        var en = CultureInfo.GetCultureInfo("en");

        // Write the header
        worksheet.Cell(1, 1).Value = "Key";
        var cell = worksheet.Cell(1, 2);
        cell.Value = en.NativeName;
        var comment = cell.CreateComment();
        comment.AddText(en.Name);

        int colIndex = 3;
        foreach (var entry in result)
        {
            cell = worksheet.Cell(1, colIndex++);
            cell.Value = entry.Key.NativeName;
            comment = cell.CreateComment();
            comment.AddText(entry.Key.Name);
        }

        // Apply styles to the header
        var headerRange = worksheet.Range(1, 1, 1, colIndex - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Write the data
        int rowIndex = 2;
        foreach (var entry in mainFile)
        {
            worksheet.Cell(rowIndex, 1).Value = entry.Key;
            worksheet.Cell(rowIndex, 2).Value = entry.Value;
            colIndex = 3;

            foreach (var langEntry in result)
            {
                cell = worksheet.Cell(rowIndex, colIndex++);
                if (langEntry.Value.TryGetValue(entry.Key, out var value))
                {
                    cell.Value = value;
                }
                else
                {
                    cell.Value = "-";
                }
            }

            rowIndex++;
        }

        worksheet.Column(1).Hide();
        worksheet.CellsUsed().Style.Alignment.WrapText = true;
        worksheet.Columns().AdjustToContents();

        // Set max width
        int maxWidth = 100;
        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width > maxWidth)
                column.Width = maxWidth;
        }

        worksheet.Rows().AdjustToContents();

        // Freeze the headers and the first column
        worksheet.SheetView.FreezeRows(1);
        worksheet.SheetView.FreezeColumns(2);

        workbook.SaveAs(excelFilePath);
    }
}

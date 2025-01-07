using ClosedXML.Excel;
using CommandLine;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WriteMissingResourceItems;

public class Program
{
    public static async Task Main(string[] args)
    {
        var parameters = Parser.Default.ParseArguments<ApplicationParameters>(args);

        if (parameters.Tag == ParserResultType.NotParsed)
        {
            foreach (Error error in parameters.Errors)
            {
                Console.WriteLine(error);
            }
        }

        string resxFilePath = parameters.Value.ResxFile;
        var path = Path.GetDirectoryName(resxFilePath)!;
        var fileName = Path.GetFileNameWithoutExtension(resxFilePath);
        var searchPattern = fileName + ".*.resx";
        var langFiles = Directory.EnumerateFiles(path, searchPattern).ToList();

        var data = ReadExcel(parameters.Value.ExcelFile);
        var resxWriter = new ResxWriter();
        foreach (var file in langFiles)
        {
            var lang = Path.GetFileNameWithoutExtension(file).Split('.')[1];
            var culture = CultureInfo.GetCultureInfo(lang);
            if (data.TryGetValue(culture, out var translations))
            {
                Console.WriteLine("Updating " + file);
                await resxWriter.WriteResxAsync(file, translations);
            }
        }
    }

    private static Dictionary<CultureInfo, Dictionary<string, string>> ReadExcel(string filePath)
    {
        var result = new Dictionary<CultureInfo, Dictionary<string, string>>();

        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1);
            var headerRow = worksheet.Row(1);

            for (int col = 2; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
            {
                var cultureName = headerRow.Cell(col).GetComment().Text;
                var culture = CultureInfo.GetCultureInfo(cultureName);

                ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(result, culture, out bool exists);
                if (!exists)
                    value = [];

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var key = row.Cell(1).GetValue<string>();
                    var translation = row.Cell(col).GetValue<string>();
                    if (string.IsNullOrWhiteSpace(translation) || translation == "-")
                        continue;

                    value![key] = translation;
                }
            }
        }

        return result;
    }
}

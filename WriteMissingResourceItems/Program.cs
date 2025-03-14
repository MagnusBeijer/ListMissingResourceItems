using ClosedXML.Excel;
using CommandLine;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;

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
        var keysInMainFile = parameters.Value.TrimOtherFiles ? await GetKeysInMainFile(resxFilePath) : [];

        foreach (var file in langFiles)
        {
            var resxWriter = await ResxWriter.OpenAsync(file);
            var lang = Path.GetFileNameWithoutExtension(file).Split('.')[1];
            var culture = CultureInfo.GetCultureInfo(lang);
            if (data.TryGetValue(culture, out var translations))
            {
                Console.WriteLine("Updating " + file);
                resxWriter.UpdateValues(translations);
            }

            if (parameters.Value.TrimOtherFiles)
                resxWriter.Trim(keysInMainFile);

            await resxWriter.SaveAsync(file);
        }
    }

    private static async Task<HashSet<string>> GetKeysInMainFile(string resxFilePath)
    {
        using (var textReader = File.OpenText(resxFilePath))
        using (var xmlReader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true, }))
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await xmlReader.ReadAsync())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "data")
                {
                    string? key = xmlReader.GetAttribute("name");
                    if (key != null)
                    {
                        keys.Add(key);
                    }
                }
            }
            return keys;
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

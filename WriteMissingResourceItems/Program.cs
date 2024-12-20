using ClosedXML.Excel;
using CommandLine;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;

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
        foreach (var file in langFiles)
        {
            var lang = Path.GetFileNameWithoutExtension(file).Split('.')[1];
            var culture = CultureInfo.GetCultureInfo(lang);
            if (data.TryGetValue(culture, out var translations))
            {
                Console.WriteLine("Updating " + file);
                await WriteResxAsync(file, translations);
            }
        }
    }

    private static Dictionary<CultureInfo, Dictionary<string, string>> ReadExcel(string filePath)
    {
        var result = new Dictionary<CultureInfo, Dictionary<string, string>>();

        try
        {

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open excel-file: {filePath}, reason: {ex.Message}");
        }

        return result;
    }

    private static async Task WriteResxAsync(string filePath, Dictionary<string, string> values)
    {
        XDocument doc;
        await using (var stream = File.OpenRead(filePath))
            doc = await XDocument.LoadAsync(stream, System.Xml.Linq.LoadOptions.None, CancellationToken.None);

        XElement? root = doc.Element("root");

        if (root == null)
            return;

        var existingElements = root
                                .Elements("data").Select(e => (Name: e.Attribute("name")?.Value, Value: e.Element("value")))
                                .Where(x => x.Name != null && x.Value != null)
                                .ToDictionary(x => x.Name!, x => x.Value!);

        foreach (var entry in values)
        {
            if (existingElements.TryGetValue(entry.Key, out var valueElement))
            {
                valueElement.Value = entry.Value;
            }
            else
            {
                root.Add(new XElement("data",
                                      new XAttribute("name", entry.Key),
                                      new XAttribute(XNamespace.Xml + "space", "preserve"),
                                      new XElement("value", entry.Value)));
            }
        }

        await using (var stream = File.OpenWrite(filePath))
            await doc.SaveAsync(stream, System.Xml.Linq.SaveOptions.None, CancellationToken.None);
    }

}

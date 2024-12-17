using ClosedXML.Excel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WriteMissingResourceItems
{
    internal class Program
    {
        public static async Task Main()
        {
            string filePath = @"C:\R\iXDeveloper\Resources\ResourcesIde\Texts\TextsIde.resx";
            var path = Path.GetDirectoryName(filePath)!;
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var searchPattern = fileName + ".*.resx";
            var langFiles = Directory.EnumerateFiles(path, searchPattern).ToList();

            var data = ReadExcel(@"c:\temp\out.xlsx");
            foreach (var file in langFiles)
            {
                var lang = Path.GetFileNameWithoutExtension(file).Split('.')[1];
                var culture = CultureInfo.GetCultureInfo(lang);
                if (data.TryGetValue(culture, out var translations))
                {
                    Console.WriteLine("Updating " + file);
                    await WriteResx(file, translations);
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

        private static async Task WriteResx(string filePath, Dictionary<string, string> values)
        {
            XDocument doc = XDocument.Load(filePath);
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

            await using var stream = File.OpenWrite(filePath);
            await doc.SaveAsync(stream, System.Xml.Linq.SaveOptions.None, CancellationToken.None);
        }

    }
}

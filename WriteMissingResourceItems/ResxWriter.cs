using System.Globalization;
using System.Xml.Linq;

namespace WriteMissingResourceItems;

public class ResxWriter
{
    public async Task WriteResxAsync(string mainRexFile, Dictionary<CultureInfo, Dictionary<string, string>> data)
    {
        var path = Path.GetDirectoryName(mainRexFile)!;
        var fileName = Path.GetFileNameWithoutExtension(mainRexFile);
        var searchPattern = fileName + ".*.resx";
        var langFiles = Directory.EnumerateFiles(path, searchPattern).ToList();

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

    private static async Task WriteResxAsync(string filePath, Dictionary<string, string> values)
    {
        XDocument doc;
        await using (var stream = File.OpenRead(filePath))
            doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

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

        await using (var stream = File.Create(filePath))
            await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }
}

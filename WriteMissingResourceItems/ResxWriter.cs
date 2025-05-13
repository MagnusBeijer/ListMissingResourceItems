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
        var mainKeys = await GetMainKeys(mainRexFile);

        foreach (var file in langFiles)
        {
            var lang = Path.GetFileNameWithoutExtension(file).Split('.')[1];
            var culture = CultureInfo.GetCultureInfo(lang);
            if (data.TryGetValue(culture, out var translations))
            {
                Console.WriteLine("Updating " + file);
                await WriteResxAsync(file, translations, mainKeys);
            }
        }
    }

    private static async Task<HashSet<string>> GetMainKeys(string mainRexFile)
    {
        await using var stream = File.OpenRead(mainRexFile);
        return (await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None))
                    .Element("root")!
                    .Elements("data")
                    .Select(e => e.Attribute("name")?.Value)
                    .Where(key => key != null)
                    .Select(key => key!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static async Task WriteResxAsync(string filePath, Dictionary<string, string> values, HashSet<string> mainKeys)
    {
        XDocument doc;
        await using (var stream = File.OpenRead(filePath))
            doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        var root = doc.Element("root");

        if (root == null)
            return;

        var existingElements = root
                                .Elements("data").Select(e => (Name: e.Attribute("name")?.Value, Value: e.Element("value")))
                                .Where(x => x.Name != null && x.Value != null)
                                .ToDictionary(x => x.Name!, x => x.Value!, StringComparer.OrdinalIgnoreCase);

        AddOrUpdate(values, root, existingElements);
        Remove(mainKeys, existingElements);

        await using (var stream = File.Create(filePath))
            await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }

    private static void Remove(HashSet<string> mainKeys, Dictionary<string, XElement> existingElements)
    {
        foreach (var item in existingElements.Where(x => !mainKeys.Contains(x.Key)))
        {
            item.Value.Parent!.Remove();
        }
    }

    private static void AddOrUpdate(Dictionary<string, string> values, XElement root, Dictionary<string, XElement> existingElements)
    {
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
    }
}

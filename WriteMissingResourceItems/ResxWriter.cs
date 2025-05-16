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

    private static async Task WriteResxAsync(string filePath, Dictionary<string, string> translations, HashSet<string> mainKeys)
    {
        XDocument doc;
        await using (var stream = File.OpenRead(filePath))
            doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        var root = doc.Element("root");

        if (root == null)
            return;

        var existingElements = GetExistingElements(root);

        AddOrUpdate(existingElements, translations, root);
        Remove(existingElements, mainKeys);

        await using (var stream = File.Create(filePath))
            await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }

    private static Dictionary<string, XElement> GetExistingElements(XElement root)
    {
        return root
                .Elements("data").Select(e => (Name: e.Attribute("name")?.Value, Value: e.Element("value")))
                .Where(x => x.Name != null && x.Value != null)
                .ToDictionary(x => x.Name!, x => x.Value!, StringComparer.OrdinalIgnoreCase);
    }

    private static void Remove(Dictionary<string, XElement> existingElements, HashSet<string> mainKeys)
    {
        foreach (var item in existingElements.Where(x => !mainKeys.Contains(x.Key)))
        {
            item.Value.Parent!.Remove();
        }
    }

    private static void AddOrUpdate(Dictionary<string, XElement> existingElements, Dictionary<string, string> translations, XElement root)
    {
        foreach (var translation in translations)
        {
            if (existingElements.TryGetValue(translation.Key, out var valueElement))
            {
                valueElement.Value = translation.Value;
            }
            else
            {
                root.Add(new XElement("data",
                                      new XAttribute("name", translation.Key),
                                      new XAttribute(XNamespace.Xml + "space", "preserve"),
                                      new XElement("value", translation.Value)));
            }
        }
    }
}

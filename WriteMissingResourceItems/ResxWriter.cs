using System.Xml.Linq;

namespace WriteMissingResourceItems;

public class ResxWriter
{
    private readonly XDocument _doc;
    private readonly XElement _root;
    private readonly Dictionary<string, XElement> _existingElements;

    private ResxWriter(XDocument doc, XElement root, Dictionary<string, XElement> existingElements)
    {
        _doc = doc;
        _root = root;
        _existingElements = existingElements;
    }

    public static async Task<ResxWriter> OpenAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        var root = doc.Element("root");

        if (root == null)
            throw new InvalidOperationException("The root element is missing in the .resx file.");

        var existingElements = GetExistingElements(root);

        return new ResxWriter(doc, root, existingElements);
    }

    private static Dictionary<string, XElement> GetExistingElements(XElement root)
    {
        return root
            .Elements("data")
            .Select(e => (Name: e.Attribute("name")?.Value, Value: e.Element("value")))
            .Where(x => x.Name != null && x.Value != null)
            .ToDictionary(x => x.Name!, x => x.Value!, StringComparer.OrdinalIgnoreCase);
    }

    public void Trim(HashSet<string> keysInMainFile)
    {
        foreach (var key in _existingElements.Keys.Where(key => !keysInMainFile.Contains(key)).ToList())
        {
            RemoveElement(key);
        }
    }

    public async Task SaveAsync(string filePath)
    {
        await using var stream = File.Create(filePath);
        await _doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }

    public void UpdateValues(Dictionary<string, string> values)
    {
        foreach (var entry in values)
        {
            if (_existingElements.TryGetValue(entry.Key, out var valueElement))
            {
                valueElement.Value = entry.Value;
            }
            else
            {
                AddElement(entry.Key, entry.Value);
            }
        }
    }

    private void AddElement(string key, string value)
    {
        var newElement = new XElement("data",
                                      new XAttribute("name", key),
                                      new XAttribute(XNamespace.Xml + "space", "preserve"),
                                      new XElement("value", value));
        _root.Add(newElement);
        _existingElements[key] = newElement.Element("value")!;
    }

    private void RemoveElement(string key)
    {
        var valueElement = _existingElements[key];
        valueElement.Parent?.Remove();
        _existingElements.Remove(key);
    }
}

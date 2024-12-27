using System.Xml;

namespace ListMissingResourceItems;

public class ResxReader
{
    public async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(string filePath)
    {
        var textReader = File.OpenText(filePath);
        await foreach (var item in ReadResxFileAsync(textReader))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(TextReader textReader)
    {
        using (textReader)
        using (var reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true, }))
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
                {
                    string? key = reader.GetAttribute("name");

                    if (key != null)
                    {
                        string? value = null;
                        if (reader.ReadToDescendant("value"))
                        {
                            value = await reader.ReadElementContentAsStringAsync();
                        }

                        yield return (key, value);
                    }
                }
            }
        }
    }
}

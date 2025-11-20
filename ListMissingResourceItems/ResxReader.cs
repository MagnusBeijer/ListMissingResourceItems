using System.Xml;

namespace ListMissingResourceItems;

public class ResxReader
{
    public IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(string filePath)
    {
        var textReader = File.OpenText(filePath);
        return ReadResxFileAsync(textReader);
    }

    public async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(TextReader textReader)
    {
        using (textReader)
        {
            if (textReader.Peek() == -1)
                yield break;

            using (var xmlReader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true, }))
            {
                while (await xmlReader.ReadAsync())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "data")
                    {
                        string? key = xmlReader.GetAttribute("name");

                        if (key != null)
                        {
                            string? value = null;
                            if (xmlReader.ReadToDescendant("value"))
                            {
                                value = await xmlReader.ReadElementContentAsStringAsync();
                            }

                            yield return (key, value);
                        }
                    }
                }
            }
        }
    }
}

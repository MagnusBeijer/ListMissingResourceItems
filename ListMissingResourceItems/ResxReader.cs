using System.Xml;

namespace ListMissingResourceItems;

public class ResxReader
{
    public async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(string filePath)
    {
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

        await foreach (var item in ReadResxFileAsync(new StreamReader(stream)))
            yield return item;
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

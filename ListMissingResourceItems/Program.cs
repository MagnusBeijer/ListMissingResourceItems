using System.Globalization;
using System.Xml;

partial class Program
{
    private const int FetchConcurrency = 10;
    private static readonly ITranslator translator = new GoogleTranslateLite();
    private static readonly ExcelWriter excelWriter = new ExcelWriter();

    static async Task Main(string[] args)
    {
        var filePath = @"C:\R\iXDeveloper\Resources\ResourcesIde\Texts\TextsIde.resx";
        var nrOfItemsToRead = 11;
        var mainFile = await ReadResxFileAsync(filePath).TakeLast(nrOfItemsToRead).ToDictionaryAsync(x => x.key, x => x.value);

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var searchPattern = fileName + ".*.resx";
        var path = Path.GetDirectoryName(filePath)!;
        var result = new Dictionary<string, Dictionary<string, string>>();
        var from = CultureInfo.GetCultureInfo("en");
        var fetchBuffer = new Dictionary<string, Task<string>>(FetchConcurrency);
        var langFiles = Directory.EnumerateFiles(path, searchPattern).ToList();
        var nrOfTexts = langFiles.Count * mainFile.Count;
        var fetched = 0;
        Console.WriteLine($"Fetching translations for {nrOfTexts} texts");


        foreach (var file in langFiles)
        {
            var lang = Path.GetFileNameWithoutExtension(file).Split('.')[1];
            var translationsExists = await ReadResxFileAsync(file).Where(x => !string.IsNullOrWhiteSpace(x.value)).Select(x => x.key).ToHashSetAsync();
            var localResult = new Dictionary<string, string>();
            var to = CultureInfo.GetCultureInfo(lang);

            foreach (var entry in mainFile)
            {
                if (!string.IsNullOrEmpty(entry.Value) && !translationsExists.Contains(entry.Key))
                {
                    var translationTask = translator.Translate(from, to, entry.Value, CancellationToken.None);
                    fetchBuffer.Add(entry.Key, translationTask);
                }
                else
                {
                    fetched++;
                }

                if (fetchBuffer.Count == FetchConcurrency)
                {
                    await FillResultFromBuffer(fetchBuffer, localResult);
                    fetched += FetchConcurrency;
                    Console.WriteLine($"{fetched} Fetched");
                }
            }

            fetched += fetchBuffer.Count;
            await FillResultFromBuffer(fetchBuffer, localResult);
            Console.WriteLine($"{fetched} Fetched");

            result.Add(lang, localResult);
        }

        excelWriter.Write(mainFile, result);
    }

    private static async Task FillResultFromBuffer(Dictionary<string, Task<string>> fetchBuffer, Dictionary<string, string> localResult)
    {
        foreach (var item in fetchBuffer)
        {
            localResult.Add(item.Key, await item.Value);
        }
        fetchBuffer.Clear();
    }

    public static async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(string filePath)
    {
        using (XmlReader reader = XmlReader.Create(filePath, new XmlReaderSettings { Async = true }))
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
                {
                    string? key = reader.GetAttribute("name");
                    string? value = null;

                    if (reader.ReadToDescendant("value"))
                    {
                        value = await reader.ReadElementContentAsStringAsync();
                    }

                    if (key != null)
                    {
                        yield return (key, value);
                    }
                }
            }
        }
    }
}

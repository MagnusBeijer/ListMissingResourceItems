using CommandLine;
using ListMissingResourceItems.Translators;
using System.Globalization;
using System.Xml;

namespace ListMissingResourceItems;

partial class Program
{
    private const int FetchConcurrency = 10;
    private static readonly ExcelWriter _excelWriter = new ExcelWriter();

    static async Task Main(string[] args)
    {
        ParserResult<ApplicationParameters> parameters = Parser.Default.ParseArguments<ApplicationParameters>(args);

        if (parameters.Tag == ParserResultType.NotParsed)
        {
            foreach (Error error in parameters.Errors)
            {
                Console.WriteLine(error);
            }
        }

        var resxFilePath = parameters.Value.ResxFile;
        var nrOfItemsToRead = parameters.Value.NrOfItemsToRead;
        var translator = TranslatorFactory(parameters.Value.Translator);

        var mainFile = nrOfItemsToRead == null ?
            await ReadResxFileAsync(resxFilePath).ToDictionaryAsync(x => x.key, x => x.value) :
            await ReadResxFileAsync(resxFilePath).TakeLast(nrOfItemsToRead.Value).ToDictionaryAsync(x => x.key, x => x.value);

        var result = await GetCultureStrings(resxFilePath, translator, mainFile);

        _excelWriter.Write(mainFile, result, parameters.Value.ExcelFile);
    }

    private static async Task<Dictionary<CultureInfo, Dictionary<string, string>>> GetCultureStrings(string resxFilePath, ITranslator translator, Dictionary<string, string?> mainFile)
    {
        var result = new Dictionary<CultureInfo, Dictionary<string, string>>();
        var fileName = Path.GetFileNameWithoutExtension(resxFilePath);
        var searchPattern = fileName + ".*.resx";
        var path = Path.GetDirectoryName(resxFilePath)!;

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
                    var translationTask = translator.TranslateAsync(from, to, entry.Value, CancellationToken.None);
                    fetchBuffer.Add(entry.Key, translationTask);
                }
                else
                {
                    fetched++; // No need to do any translation
                }

                if (fetchBuffer.Count == FetchConcurrency)
                {
                    await FillResultFromBufferAsync(fetchBuffer, localResult);
                    fetched += FetchConcurrency;
                    Console.WriteLine($"{fetched} Fetched");
                }
            }

            fetched += fetchBuffer.Count;
            await FillResultFromBufferAsync(fetchBuffer, localResult);
            Console.WriteLine($"{fetched} Fetched");

            result.Add(to, localResult);
        }

        return result;
    }

    public static ITranslator TranslatorFactory(string translator)
    {
        static string GetGoogleAuthKey() => File.ReadAllText("GoogleAuthKey.txt");

        return translator switch
        {
            "GoogleMlTranslator" => new GoogleMlTranslator(GetGoogleAuthKey()),
            _ or "GoogleTranslateLite" => new GoogleTranslateLite(),
        };
    }

    private static async Task FillResultFromBufferAsync(Dictionary<string, Task<string>> fetchBuffer, Dictionary<string, string> localResult)
    {
        foreach (var item in fetchBuffer)
        {
            localResult.Add(item.Key, await item.Value);
        }
        fetchBuffer.Clear();
    }

    public static async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(string filePath)
    {
        using XmlReader reader = XmlReader.Create(filePath, new XmlReaderSettings { Async = true });
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

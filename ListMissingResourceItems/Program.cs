using System.Diagnostics;
using System.Globalization;
using System.Xml;
using CommandLine;
using ListMissingResourceItems.Translators;

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

        var relativeResxFilePath = parameters.Value.RelativeResxFilePath;
        var repoPath = parameters.Value.RepoPath;
        var remoteBranch = parameters.Value.RemoteBranch;
        var translator = TranslatorFactory(parameters.Value.Translator);
        var resxFilePath = Path.Combine(repoPath, relativeResxFilePath.Replace('/', '\\').TrimStart('\\'));

        var mainFile = await CompareFiles(relativeResxFilePath, repoPath, remoteBranch, resxFilePath)
                                .Where(x => !string.IsNullOrWhiteSpace(x.value))
                                .ToDictionaryAsync(x => x.key, x => x.value!);

        var result = await GetCultureStrings(resxFilePath, translator, mainFile);

        _excelWriter.Write(mainFile, result, parameters.Value.ExcelFile);
        OpenExcelFile(parameters);
    }

    private static void OpenExcelFile(ParserResult<ApplicationParameters> parameters)
    {
        using var process = new Process();
        process.StartInfo.FileName = parameters.Value.ExcelFile;
        process.StartInfo.UseShellExecute = true;

        process.Start();
    }

    private static async IAsyncEnumerable<(string key, string? value)> CompareFiles(string relativeResxFilePath, string repoPath, string remoteBranch, string resxFilePath)
    {
        var gitCommand = $"show {remoteBranch}:" + relativeResxFilePath.Replace('\\', '/').TrimStart('/');

        using var process = new Process();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = gitCommand;
        process.StartInfo.WorkingDirectory = repoPath;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        var remoterBranchFile = await ReadResxFileAsync(process.StandardOutput).ToDictionaryAsync(x => x.key, x => x.value);
        var myBranchFile = ReadResxFileAsync(resxFilePath);

        await foreach (var (key, value) in myBranchFile)
        {
            if (!remoterBranchFile.TryGetValue(key, out var otherValue) || otherValue != value)
                yield return (key, value);
        }
        process.WaitForExit();
    }

    private static async Task<Dictionary<CultureInfo, Dictionary<string, string>>> GetCultureStrings(string resxFilePath, ITranslator translator, Dictionary<string, string> mainFile)
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
            var localResult = new Dictionary<string, string>();
            var to = CultureInfo.GetCultureInfo(lang);

            foreach (var entry in mainFile)
            {
                var translationTask = translator.TranslateAsync(from, to, entry.Value!, CancellationToken.None);
                fetchBuffer.Add(entry.Key, translationTask);

                if (fetchBuffer.Count == FetchConcurrency)
                {
                    await FillResultFromBufferAsync(fetchBuffer, localResult);
                    fetched += FetchConcurrency;
                    Console.WriteLine($"{fetched} Fetched");
                }
            }

            if (fetchBuffer.Count > 0)
            {
                fetched += fetchBuffer.Count;
                await FillResultFromBufferAsync(fetchBuffer, localResult);
                Console.WriteLine($"{fetched} Fetched");

            }

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

    public static IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(string filePath)
    {
        var textReader = File.OpenText(filePath);
        return ReadResxFileAsync(textReader);
    }

    public static async IAsyncEnumerable<(string key, string? value)> ReadResxFileAsync(TextReader textReader)
    {
        using XmlReader reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true, });
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
        textReader.Dispose();
    }
}

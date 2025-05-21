using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommandLine;
using ListMissingResourceItems.Translators;
using WriteMissingResourceItems;

namespace ListMissingResourceItems;

partial class Program
{
    private const int FetchConcurrency = 10;
    private static readonly ResxReader _resxReader = new ResxReader();

    static async Task Main(string[] args)
    {
        if (!await IsGitInstalledAsync())
        {
            Console.WriteLine("Error: Git is not installed. Please install Git to continue.");
            return;
        }

        ParserResult<ApplicationParameters> parameters = Parser.Default.ParseArguments<ApplicationParameters>(args);

        if (parameters.Tag == ParserResultType.NotParsed)
        {
            foreach (Error error in parameters.Errors)
            {
                Console.WriteLine(error);
            }
            return;
        }

        if (parameters.Value.TargetExcelFile == null && parameters.Value.TargetResxFile == null)
        {
            Console.WriteLine("Error: Either Target-Excel-File or Target-Resx-File must be specified.");
            return;
        }

        var sourceResxFile = parameters.Value.SourceResxFile;
        var translator = TranslatorFactory(parameters.Value.Translator);
        var remoteBranch = parameters.Value.RemoteBranch;

        var mainFile = await (remoteBranch == null
                                    ? _resxReader.ReadResxFileAsync(sourceResxFile)
                                    : GetDiffOfResxBetweenBranchesAsync(remoteBranch, sourceResxFile))
                                .Where(x => !string.IsNullOrWhiteSpace(x.value))
                                .ToDictionaryAsync(x => x.key, x => x.value!);

        var result = await GetCultureStringsAsync(sourceResxFile, translator, mainFile);

        if (parameters.Value.TargetExcelFile != null)
        {
            var excelWriter = new ExcelWriter();
            excelWriter.Write(mainFile, result, parameters.Value.TargetExcelFile);

            if (parameters.Value.OpenExcel)
            {
                Console.WriteLine("Opening Excel file to display the result...");
                OpenExcelFile(parameters);
            }
        }

        if (parameters.Value.TargetResxFile != null)
        {
            var resxWriter = new ResxWriter();
            await resxWriter.WriteResxAsync(parameters.Value.TargetResxFile, result);
        }
    }

    private static void OpenExcelFile(ParserResult<ApplicationParameters> parameters)
    {
        using var process = new Process();
        process.StartInfo.FileName = parameters.Value.TargetExcelFile;
        process.StartInfo.UseShellExecute = true;

        process.Start();
    }

    private static async Task<string> GetRepoPathAsync(string resxFilePath)
    {
        using var process = new Process();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = "rev-parse --show-toplevel";
        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(resxFilePath);
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

        process.Start();
        var path = (await process.StandardOutput.ReadToEndAsync()).Trim().Replace('/', '\\');
        await process.WaitForExitAsync();

        return path;
    }

    private static async IAsyncEnumerable<(string key, string? value)> GetDiffOfResxBetweenBranchesAsync(string remoteBranch, string resxFilePath)
    {
        var repoPath = await GetRepoPathAsync(resxFilePath);
        var relativeResxFilePath = resxFilePath[(repoPath.Length + 1)..];
        var gitCommand = $"show {remoteBranch}:" + relativeResxFilePath.Replace('\\', '/').TrimStart('/');

        using var process = new Process();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = gitCommand;
        process.StartInfo.WorkingDirectory = repoPath;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

        process.Start();

        var remoterBranchFile = await _resxReader.ReadResxFileAsync(process.StandardOutput).ToDictionaryAsync(x => x.key, x => x.value);
        var myBranchFile = _resxReader.ReadResxFileAsync(resxFilePath);

        await foreach (var (key, value) in myBranchFile)
        {
            if (!remoterBranchFile.TryGetValue(key, out var otherValue) || otherValue != value)
                yield return (key, value);
        }
        await process.WaitForExitAsync();
    }

    private static async Task<Dictionary<CultureInfo, Dictionary<string, string>>> GetCultureStringsAsync(string resxFilePath, ITranslator translator, Dictionary<string, string> mainFile)
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
        static string GetGoogleAuthKey()
        {
            if (!File.Exists("GoogleAuthKey.txt"))
            {
                Console.WriteLine("GoogleAuthKey.txt file not found. Please create it with your Google API key. The file should be placed in the same directory as the application exe file.");
                throw new FileNotFoundException("GoogleAuthKey.txt file not found.");
            }
            return File.ReadAllText("GoogleAuthKey.txt");
        }

        return translator switch
        {
            "GoogleMlTranslator" => new GoogleMlTranslator(GetGoogleAuthKey()),
            "GoogleTranslateLite" => new GoogleTranslateLite(),
            _ => throw new ArgumentException($"Unknown translator: {translator}. Available translators are: GoogleMlTranslator, GoogleTranslateLite")
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

    private static async Task<bool> IsGitInstalledAsync()
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = "--version";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

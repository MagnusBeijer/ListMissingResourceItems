using CommandLine;

namespace ListMissingResourceItems;

public class ApplicationParameters
{
    [Option("target-excel-file", Required = true)]
    public required string ExcelFile { get; set; }

    [Option("relative-resx-filePath", Required = true)]
    public required string RelativeResxFilePath { get; set; }

    [Option("repository-path", Required = true)]
    public required string RepoPath { get; set; }

    [Option("remote-branch-name", Required = true)]
    public required string RemoteBranch { get; set; }

    [Option("translator", HelpText = "Can be either GoogleMlTranslator or GoogleTranslateLite (default)")]
    public string Translator { get; set; } = "GoogleTranslateLite";
}

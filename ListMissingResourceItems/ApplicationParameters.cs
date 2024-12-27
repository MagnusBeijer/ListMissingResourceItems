using CommandLine;

namespace ListMissingResourceItems;

public class ApplicationParameters
{
    [Option("target-excel-file", Required = true)]
    public required string ExcelFile { get; set; }

    [Option("source-resx-file", Required = true)]
    public required string SourceResxFile { get; set; }

    [Option("remote-branch-name", Required = true)]
    public required string RemoteBranch { get; set; }

    [Option("translator", HelpText = "Can be either GoogleMlTranslator or GoogleTranslateLite (default)")]
    public string Translator { get; set; } = "GoogleTranslateLite";
}

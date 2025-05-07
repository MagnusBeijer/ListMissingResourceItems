using CommandLine;

namespace ListMissingResourceItems;

public class ApplicationParameters
{
    [Option("target-excel-file", Required = false)]
    public string? TargetExcelFile { get; set; }

    [Option("target-resx-file", Required = false)]
    public string? TargetResxFile { get; set; }

    [Option("source-resx-file", Required = true)]
    public required string SourceResxFile { get; set; }

    [Option("remote-branch-name", Required = true)]
    public required string RemoteBranch { get; set; }

    [Option("translator", Required = false)]
    public string Translator { get; set; } = "GoogleTranslateLite";

    [Option("open-excel", Required = false, HelpText = "Indicates if Excel should be opened to display the result after the file has been created")]
    public bool OpenExcel { get; set; } = false;
}

using CommandLine;

namespace ListMissingResourceItems;

public class ApplicationParameters
{
    [Option("target-excel-file", Required = false, HelpText = "Path to the Excel file to save the result to")]
    public string? TargetExcelFile { get; set; }

    [Option("target-resx-file", Required = false, HelpText = "Path to the main resx file to save the result to")]
    public string? TargetResxFile { get; set; }

    [Option("source-resx-file", Required = true, HelpText = "Path to the the main resx file to use as source")]
    public required string SourceResxFile { get; set; }

    [Option("remote-branch-name", Required = true, HelpText = "Name of the remote branch to compare the resx file with")]
    public required string RemoteBranch { get; set; }

    [Option("translator", Default = "GoogleTranslateLite", Required = false, HelpText = "Can be either GoogleMlTranslator or GoogleTranslateLite")]
    public string Translator { get; set; } = "GoogleTranslateLite";

    [Option("open-excel", Default = false, Required = false, HelpText = "Indicates if Excel should be opened to display the result after the file has been created")]
    public bool OpenExcel { get; set; } = false;
}

using CommandLine;

namespace WriteMissingResourceItems;

public class ApplicationParameters
{
    [Option("target-resx-file", Required = true)]
    public required string ResxFile { get; set; }

    [Option("source-excel-file", Required = true)]
    public required string ExcelFile { get; set; }

    [Option("trim-other-files", Required = false, Default = false, HelpText = "Remove entries from releated resx files if they do not exist in the main resx file.")]
    public required bool TrimOtherFiles { get; set; } = false;
}

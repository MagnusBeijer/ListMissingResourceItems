using CommandLine;

namespace ListMissingResourceItems;

public class ApplicationParameters
{
    [Option("source-resx-file", Required = true)]
    public required string ResxFile { get; set; }

    [Option("target-excel-file", Required = true)]
    public required string ExcelFile { get; set; }

    [Option("items-to-read", HelpText = "The nr. of items to read from the end of the resx file")]
    public int? NrOfItemsToRead { get; set; }

    [Option("translator", HelpText = "Can be either GoogleMlTranslator or GoogleTranslateLite (default)")]
    public string Translator { get; set; } = "GoogleTranslateLite";
}

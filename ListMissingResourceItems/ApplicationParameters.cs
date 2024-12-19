using CommandLine;

namespace ListMissingResourceItems;

public class ApplicationParameters
{
    [Option("source-resx-file", Required = true)]
    public required string ResxFile { get; set; }

    [Option("target-excel-file", Required = true)]
    public required string ExcelFile { get; set; }

    [Option("items-to-read", HelpText = "The nr. of items to read from the end. 'take-from-key' is evaluated before this one if used together.")]
    public int? NrOfItemsToRead { get; set; }

    [Option("translator", HelpText = "Can be either GoogleMlTranslator or GoogleTranslateLite (default)")]
    public string Translator { get; set; } = "GoogleTranslateLite";

    [Option("take-from-key", HelpText = "The key to start reading from")]
    public string? TakeFromKey { get; set; }
}

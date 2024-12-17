using CommandLine;

namespace ListMissingResourceItems
{
    public class ApplicationParameters
    {
        [Option("source-resx-file", Required = true)]
        public required string ResxFile { get; set; }

        [Option("target-excel-file", Required = true)]
        public required string ExcelFile { get; set; }
    }
}

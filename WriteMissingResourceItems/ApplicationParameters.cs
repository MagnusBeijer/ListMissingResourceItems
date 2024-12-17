using CommandLine;

namespace WriteMissingResourceItems
{
    public class ApplicationParameters
    {
        [Option("target-resx-file", Required = true)]
        public required string ResxFile { get; set; }

        [Option("source-excel-file", Required = true)]
        public required string ExcelFile { get; set; }
    }
}

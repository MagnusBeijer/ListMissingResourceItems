using System.Globalization;

namespace ListMissingResourceItems;
partial class Program
{
    public interface ITranslator
    {
        Task<string> Translate(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken);
    }
}

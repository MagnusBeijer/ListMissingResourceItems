using System.Globalization;

namespace ListMissingResourceItems;
partial class Program
{
    public interface ITranslator
    {
        Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken);
    }
}

using System.Globalization;

namespace ListMissingResourceItems;
public interface ITranslator
{
    Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken);
}
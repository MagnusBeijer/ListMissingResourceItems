using System.Globalization;

namespace ListMissingResourceItems.Translators;
public interface ITranslator
{
    Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken);
}
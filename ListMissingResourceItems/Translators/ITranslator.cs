using System.Globalization;

namespace ListMissingResourceItems.Translators;

/// <summary>
/// Defines a contract for translating text from one language to another.
/// </summary>
public interface ITranslator
{
    /// <summary>
    /// Translates the specified text from the source language to the target language.
    /// </summary>
    /// <param name="from">The source language.</param>
    /// <param name="to">The target language.</param>
    /// <param name="textToTranslate">The text to translate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The translated text.</returns>
    Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string textToTranslate, CancellationToken cancellationToken);
}

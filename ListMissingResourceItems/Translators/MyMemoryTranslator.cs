using System.Globalization;
using System.Text.Json;

namespace ListMissingResourceItems.Translators;

internal class MyMemoryTranslator : ITranslator
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken)
    {
        var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(value)}&langpair={from.TwoLetterISOLanguageName}|{to.TwoLetterISOLanguageName}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("responseData")
            .GetProperty("translatedText")
            .GetString()!;
    }
}


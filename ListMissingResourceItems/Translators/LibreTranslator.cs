using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ListMissingResourceItems.Translators;
internal class LibreTranslator : ITranslator
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            q = value,
            source = from.TwoLetterISOLanguageName,
            target = to.TwoLetterISOLanguageName
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://libretranslate.com/translate", content, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

using System.Globalization;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Cloud.Translation.V2;

namespace ListMissingResourceItems.Translators;

internal class GoogleMlTranslator : ITranslator
{
    private static string? _authKey;
    private static readonly HashSet<string> _zh = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "zh-hant", "zh-cht", "zh-hk", "zh-mo", "zh-tw" };
    private readonly TranslationClientImpl _client;

    public GoogleMlTranslator(string authKey)
    {
        _authKey = authKey;
        var service = new TranslateService(new BaseClientService.Initializer { ApiKey = _authKey });
        _client = new TranslationClientImpl(service, TranslationModel.ServiceDefault);
    }

    public async Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string textToTranslate, CancellationToken cancellationToken)
    {
        //parameter "from" is ignored since English is default
        return (await ErrorHandlingDecorator.ExecuteWithHandling(() => _client.TranslateTextAsync(textToTranslate, GoogleLangCode(to)))).TranslatedText;
    }

    private static string GoogleLangCode(CultureInfo cultureInfo)
    {
        var iso1 = cultureInfo.TwoLetterISOLanguageName;
        var name = cultureInfo.Name;

        if (string.Equals(iso1, "zh", StringComparison.OrdinalIgnoreCase))
            return _zh.Contains(name) ? "zh-TW" : "zh-CN";

        if (string.Equals(name, "haw-us", StringComparison.OrdinalIgnoreCase))
            return "haw";

        return iso1;
    }
}

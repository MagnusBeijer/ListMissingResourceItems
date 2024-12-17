using System.Globalization;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Cloud.Translation.V2;
using static ListMissingResourceItems.Program;

namespace ListMissingResourceItems;
internal class GoogleMlTranslator : ITranslator
{
    private static string? _AuthKey;
    private static readonly HashSet<string> _zh = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "zh-hant", "zh-cht", "zh-hk", "zh-mo", "zh-tw" };
    
    public GoogleMlTranslator()
    {
        _AuthKey = "YOUR API KEY HERE";//TODO read from secret or else
    }

    public async Task<string> TranslateAsync(CultureInfo from, CultureInfo to, string textToTranslate, CancellationToken cancellationToken)
    {
        //from ignored english is default
        return await ErrorHandlingDecorator.ExecuteWithHandling(async () =>
            {
                var service = new TranslateService(new BaseClientService.Initializer { ApiKey = _AuthKey });
                var client = new TranslationClientImpl(service, TranslationModel.ServiceDefault);
                var result = client.TranslateText(textToTranslate, GoogleLangCode(to));

                return result.TranslatedText;
            });
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

using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Ui.Settings;

namespace PersonalFinance.Ui.Features.ImportExpenses.Services;

public sealed class OcrLanguageProvider : IOcrLanguageProvider
{
    private readonly AppSettingsStore _settingsStore;

    public OcrLanguageProvider(AppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    public string GetLanguageCode()
    {
        var settings = _settingsStore.LoadOrDefault();
        if (!string.IsNullOrWhiteSpace(settings.OcrLanguageCode))
        {
            return Normalize(settings.OcrLanguageCode);
        }

        return MapCultureToLanguage(settings.CultureName);
    }

    private static string MapCultureToLanguage(string culture)
    {
        return culture.Equals("pt-BR", StringComparison.OrdinalIgnoreCase) ? "por" : "eng";
    }

    private static string Normalize(string languageCode)
    {
        return languageCode.Trim().ToLowerInvariant();
    }
}

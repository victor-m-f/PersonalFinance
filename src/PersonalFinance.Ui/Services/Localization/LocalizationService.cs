using System.Globalization;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Settings;
using System.Windows;

namespace PersonalFinance.Ui.Services.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly AppSettingsStore _settingsStore;
    private readonly IReadOnlyList<CultureInfo> _supportedCultures =
    [
        new("en-US"),
        new("pt-BR")
    ];

    public CultureInfo CurrentCulture { get; private set; }
    public IReadOnlyList<CultureInfo> SupportedCultures => _supportedCultures;

    public LocalizationService(AppSettingsStore settingsStore, ILogger<LocalizationService> logger)
    {
        _settingsStore = settingsStore;
        _logger = logger;
        CurrentCulture = CultureInfo.GetCultureInfo("en-US");
    }
    public void InitializeFromSettings()
    {
        var settings = _settingsStore.LoadOrDefault();
        _logger.LogInformation("Initializing localization with {Culture}", settings.CultureName);
        SetCulture(settings.CultureName);
    }

    public void SetCulture(string cultureName)
    {
        var culture = GetSupportedCulture(cultureName);
        CurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        SwapLocalizationDictionary(culture.Name);

        _logger.LogInformation("Localization set to {Culture}", culture.Name);

        var settings = _settingsStore.LoadOrDefault();
        settings.CultureName = culture.Name;
        _settingsStore.Save(settings);
    }

    private CultureInfo GetSupportedCulture(string cultureName)
    {
        for (var i = 0; i < _supportedCultures.Count; i++)
        {
            var candidate = _supportedCultures[i];
            if (string.Equals(candidate.Name, cultureName, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        _logger.LogWarning("Unsupported culture {Culture}, falling back to en-US", cultureName);
        return CultureInfo.GetCultureInfo("en-US");
    }

    private static void SwapLocalizationDictionary(string cultureName)
    {
        var app = System.Windows.Application.Current;
        if (app is null)
        {
            return;
        }

        var dictionaries = app.Resources.MergedDictionaries;
        ResourceDictionary? existing = null;

        for (var i = 0; i < dictionaries.Count; i++)
        {
            var source = dictionaries[i].Source?.OriginalString;
            if (string.IsNullOrWhiteSpace(source))
            {
                continue;
            }

            if (source.Contains("/Shared/Resources/Localization/Strings.", StringComparison.OrdinalIgnoreCase) ||
                source.Contains("Shared/Resources/Localization/Strings.", StringComparison.OrdinalIgnoreCase))
            {
                existing = dictionaries[i];
                break;
            }
        }

        if (existing is not null)
        {
            dictionaries.Remove(existing);
        }

        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"Shared/Resources/Localization/Strings.{cultureName}.xaml", UriKind.Relative)
        });
    }
}

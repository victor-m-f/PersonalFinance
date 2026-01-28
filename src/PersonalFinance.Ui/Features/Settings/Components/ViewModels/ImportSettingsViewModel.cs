using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Ui.Features.ImportExpenses.Models;
using PersonalFinance.Ui.Settings;

namespace PersonalFinance.Ui.Features.Settings.Components.ViewModels;

public sealed class ImportSettingsViewModel : ObservableObject
{
    private readonly IImportSetupService _setupService;
    private readonly AppSettingsStore _settingsStore;
    private readonly ILogger<ImportSettingsViewModel> _logger;
    private ObservableCollection<ImportSetupItemVm> _setupItems = new();
    private IReadOnlyList<OcrLanguageOption> _ocrLanguageOptions = Array.Empty<OcrLanguageOption>();
    private OcrLanguageOption? _selectedOcrLanguage;

    public ObservableCollection<ImportSetupItemVm> SetupItems
    {
        get => _setupItems;
        private set => SetProperty(ref _setupItems, value);
    }

    public IReadOnlyList<OcrLanguageOption> OcrLanguageOptions
    {
        get => _ocrLanguageOptions;
        private set => SetProperty(ref _ocrLanguageOptions, value);
    }

    public OcrLanguageOption? SelectedOcrLanguage
    {
        get => _selectedOcrLanguage;
        set
        {
            if (SetProperty(ref _selectedOcrLanguage, value) && value is not null)
            {
                var settings = _settingsStore.LoadOrDefault();
                settings.OcrLanguageCode = value.Code;
                _settingsStore.Save(settings);
                _ = LoadAsync();
            }
        }
    }

    public IAsyncRelayCommand RefreshCommand { get; }

    public ImportSettingsViewModel(
        IImportSetupService setupService,
        AppSettingsStore settingsStore,
        ILogger<ImportSettingsViewModel> logger)
    {
        _setupService = setupService;
        _settingsStore = settingsStore;
        _logger = logger;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        LoadOcrLanguageOptions();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var status = await _setupService.GetStatusAsync(CancellationToken.None);
            SetupItems = new ObservableCollection<ImportSetupItemVm>(status.Select(Map));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load import setup status.");
        }
    }

    private void LoadOcrLanguageOptions()
    {
        OcrLanguageOptions = new List<OcrLanguageOption>
        {
            new("eng", GetResource("ImportSetup.Language.English", "English")),
            new("por", GetResource("ImportSetup.Language.Portuguese", "Portuguese"))
        };

        var settings = _settingsStore.LoadOrDefault();
        var code = string.IsNullOrWhiteSpace(settings.OcrLanguageCode)
            ? (settings.CultureName.Equals("pt-BR", StringComparison.OrdinalIgnoreCase) ? "por" : "eng")
            : settings.OcrLanguageCode.Trim().ToLowerInvariant();

        SelectedOcrLanguage = OcrLanguageOptions.FirstOrDefault(x => x.Code == code) ?? OcrLanguageOptions.First();
    }

    private ImportSetupItemVm Map(ImportSetupItemStatus item)
    {
        return new ImportSetupItemVm
        {
            Key = item.Key,
            Title = GetSetupTitle(item),
            LanguageCode = item.LanguageCode,
            ModelName = item.ModelName,
            StatusCode = item.Detail,
            IsRequired = item.IsRequired,
            IsInstalled = item.IsInstalled,
            StatusText = GetSetupDetail(item),
            Progress = item.IsInstalled ? 100 : 0
        };
    }

    private string GetSetupTitle(ImportSetupItemStatus item)
    {
        return item.Key switch
        {
            "tesseract" when !string.IsNullOrWhiteSpace(item.LanguageCode)
                => string.Format(GetResource("ImportSetup.Title.OcrLanguageFormat", "Tesseract OCR ({0})"), GetOcrLanguageDisplay(item.LanguageCode)),
            "tesseract" => GetResource("ImportSetup.Title.Tesseract", "Tesseract OCR"),
            "llm" when !string.IsNullOrWhiteSpace(item.ModelName)
                => string.Format(GetResource("ImportSetup.Title.LlmModelFormat", "LLM model ({0})"), item.ModelName),
            "llm" => GetResource("ImportSetup.Title.Llm", "LLM model"),
            _ => item.Title
        };
    }

    private string GetSetupDetail(ImportSetupItemStatus item)
    {
        return item.Detail switch
        {
            "Installed" => GetResource("ImportSetup.Status.Installed", "Installed"),
            "DownloadRequired" => GetResource("ImportSetup.Status.DownloadRequired", "Download required"),
            "Checking" => GetResource("ImportSetup.Status.Checking", "Checking"),
            _ => item.Detail ?? string.Empty
        };
    }

    private static string GetResource(string key, string fallback)
    {
        if (System.Windows.Application.Current?.TryFindResource(key) is string value && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    private string GetOcrLanguageDisplay(string languageCode)
    {
        var normalized = languageCode.Trim().ToLowerInvariant();
        return normalized switch
        {
            "por" => GetResource("ImportSetup.Language.Portuguese", "Portuguese"),
            "eng" => GetResource("ImportSetup.Language.English", "English"),
            _ => normalized
        };
    }
}

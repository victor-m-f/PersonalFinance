using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Services.Localization;
using PersonalFinance.Ui.Settings;
using System.Windows.Input;

namespace PersonalFinance.Ui.Features.Settings.Components.ViewModels;

public sealed class LanguageSettingsViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<LanguageSettingsViewModel> _logger;
    private bool _isInitializing;
    private string _selectedCulture = "en-US";

    public ICommand CultureSelectionChangedCommand { get; }

    public string SelectedCulture
    {
        get => _selectedCulture;
        set => SetProperty(ref _selectedCulture, value);
    }

    public LanguageSettingsViewModel(
        AppSettingsStore settingsStore,
        ILocalizationService localizationService,
        ILogger<LanguageSettingsViewModel> logger)
    {
        CultureSelectionChangedCommand = new RelayCommand<string?>(OnCultureSelectionChanged);
        _settingsStore = settingsStore;
        _localizationService = localizationService;
        _logger = logger;

        _isInitializing = true;
        var settings = _settingsStore.LoadOrDefault();
        SelectedCulture = settings.CultureName;
        _isInitializing = false;
    }

    private void OnCultureSelectionChanged(string? selectedTag)
    {
        if (_isInitializing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedTag))
        {
            return;
        }

        _logger.LogInformation("Language change requested to {Culture}", selectedTag);
        _localizationService.SetCulture(selectedTag);
    }
}

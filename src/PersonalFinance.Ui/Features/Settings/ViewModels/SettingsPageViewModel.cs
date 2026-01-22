using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Ui.Services.Localization;
using PersonalFinance.Ui.Settings;
using System.Windows.Input;

namespace PersonalFinance.Ui.Features.Settings.ViewModels;

public sealed class SettingsPageViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ILocalizationService _localizationService;
    private readonly AppSettings _settings = new();
    private readonly bool _isInitializing;
    private string _selectedCulture = "en-US";
    private string _selectedTheme = "System";

    public ICommand ThemeSelectionChangedCommand { get; }
    public ICommand CultureSelectionChangedCommand { get; }
    public string SelectedCulture
    {
        get => _selectedCulture;
        set => SetProperty(ref _selectedCulture, value);
    }
    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    public SettingsPageViewModel(
        AppSettingsStore settingsStore,
        ILocalizationService localizationService)
    {
        ThemeSelectionChangedCommand = new RelayCommand<string?>(OnThemeSelectionChanged);
        CultureSelectionChangedCommand = new RelayCommand<string?>(OnCultureSelectionChanged);
        _settingsStore = settingsStore;
        _localizationService = localizationService;

        _isInitializing = true;
        _settings = _settingsStore.LoadOrDefault();
        SelectedTheme = _settings.Theme switch
        {
            AppThemePreference.Dark => "Dark",
            AppThemePreference.Light => "Light",
            _ => "System"
        };
        SelectedCulture = _settings.CultureName;
        _isInitializing = false;
    }

    private void OnThemeSelectionChanged(string? selectedTag)
    {
        if (_isInitializing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedTag))
        {
            return;
        }

        _settings.Theme = selectedTag switch
        {
            "Dark" => AppThemePreference.Dark,
            "Light" => AppThemePreference.Light,
            _ => AppThemePreference.System
        };

        ThemeApplier.Apply(_settings);
        _settingsStore.Save(_settings);
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

        _localizationService.SetCulture(selectedTag);
    }
}

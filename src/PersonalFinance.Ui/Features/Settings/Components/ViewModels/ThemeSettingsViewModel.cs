using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Settings;
using System.Windows.Input;

namespace PersonalFinance.Ui.Features.Settings.Components.ViewModels;

public sealed class ThemeSettingsViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ILogger<ThemeSettingsViewModel> _logger;
    private AppSettings _settings = new();
    private bool _isInitializing;
    private string _selectedTheme = "System";

    public ICommand ThemeSelectionChangedCommand { get; }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    public ThemeSettingsViewModel(AppSettingsStore settingsStore, ILogger<ThemeSettingsViewModel> logger)
    {
        ThemeSelectionChangedCommand = new RelayCommand<string?>(OnThemeSelectionChanged);
        _settingsStore = settingsStore;
        _logger = logger;

        _isInitializing = true;
        _settings = _settingsStore.LoadOrDefault();
        SelectedTheme = _settings.Theme switch
        {
            AppThemePreference.Dark => "Dark",
            AppThemePreference.Light => "Light",
            _ => "System"
        };
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

        _logger.LogInformation("Theme updated to {Theme}", _settings.Theme);
        ThemeApplier.Apply(_settings);
        _settingsStore.Save(_settings);
    }
}

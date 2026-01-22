using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Helpers;
using PersonalFinance.Ui.Services.Localization;
using PersonalFinance.Ui.Settings;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace PersonalFinance.Ui.Features.Settings.ViewModels;

public sealed class SettingsPageViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<SettingsPageViewModel> _logger;
    private AppSettings _settings = new();
    private bool _isInitializing;
    private bool _verboseLogging;
    private string _selectedCulture = "en-US";
    private string _selectedTheme = "System";

    public ICommand ThemeSelectionChangedCommand { get; }
    public ICommand CultureSelectionChangedCommand { get; }
    public ICommand VerboseLoggingChangedCommand { get; }
    public ICommand OpenLogsFolderCommand { get; }
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

    public bool VerboseLogging
    {
        get => _verboseLogging;
        set => SetProperty(ref _verboseLogging, value);
    }

    public SettingsPageViewModel(
        AppSettingsStore settingsStore,
        ILocalizationService localizationService,
        ILogger<SettingsPageViewModel> logger)
    {
        ThemeSelectionChangedCommand = new RelayCommand<string?>(OnThemeSelectionChanged);
        CultureSelectionChangedCommand = new RelayCommand<string?>(OnCultureSelectionChanged);
        VerboseLoggingChangedCommand = new RelayCommand<bool?>(OnVerboseLoggingChanged);
        OpenLogsFolderCommand = new RelayCommand(OnOpenLogsFolder);
        _settingsStore = settingsStore;
        _localizationService = localizationService;
        _logger = logger;

        _isInitializing = true;
        _settings = _settingsStore.LoadOrDefault();
        SelectedTheme = _settings.Theme switch
        {
            AppThemePreference.Dark => "Dark",
            AppThemePreference.Light => "Light",
            _ => "System"
        };
        SelectedCulture = _settings.CultureName;
        VerboseLogging = _settings.VerboseLogging;
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

    private void OnVerboseLoggingChanged(bool? isEnabled)
    {
        if (_isInitializing)
        {
            return;
        }

        if (isEnabled is null)
        {
            return;
        }

        _settings.VerboseLogging = isEnabled.Value;
        _logger.LogInformation("Verbose logging set to {Verbose}", _settings.VerboseLogging);
        _settingsStore.Save(_settings);
    }

    private void OnOpenLogsFolder()
    {
        try
        {
            Directory.CreateDirectory(FileSystemPaths.LogsFolder);
            var startInfo = new ProcessStartInfo
            {
                FileName = FileSystemPaths.LogsFolder,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            _logger.LogInformation("Logs folder opened");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open logs folder");
        }
    }
}

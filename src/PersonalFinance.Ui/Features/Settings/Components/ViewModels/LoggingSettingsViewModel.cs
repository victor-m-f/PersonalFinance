using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Helpers;
using PersonalFinance.Ui.Settings;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace PersonalFinance.Ui.Features.Settings.Components.ViewModels;

public sealed class LoggingSettingsViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ILogger<LoggingSettingsViewModel> _logger;
    private AppSettings _settings = new();
    private bool _isInitializing;
    private bool _verboseLogging;

    public ICommand VerboseLoggingChangedCommand { get; }
    public ICommand OpenLogsFolderCommand { get; }

    public bool VerboseLogging
    {
        get => _verboseLogging;
        set => SetProperty(ref _verboseLogging, value);
    }

    public LoggingSettingsViewModel(AppSettingsStore settingsStore, ILogger<LoggingSettingsViewModel> logger)
    {
        VerboseLoggingChangedCommand = new RelayCommand<bool?>(OnVerboseLoggingChanged);
        OpenLogsFolderCommand = new RelayCommand(OnOpenLogsFolder);
        _settingsStore = settingsStore;
        _logger = logger;

        _isInitializing = true;
        _settings = _settingsStore.LoadOrDefault();
        VerboseLogging = _settings.VerboseLogging;
        _isInitializing = false;
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

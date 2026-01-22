using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Ui.Settings;

namespace PersonalFinance.Ui.Features.Settings.ViewModels;

public sealed class SettingsPageViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore = new();
    private readonly AppSettings _settings = new();
    private readonly bool _isInitializing;
    private string _selectedTheme = "System";

    public SettingsPageViewModel()
    {
        ThemeSelectionChangedCommand = new RelayCommand<string?>(OnThemeSelectionChanged);

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

    public ICommand ThemeSelectionChangedCommand { get; }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
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
}

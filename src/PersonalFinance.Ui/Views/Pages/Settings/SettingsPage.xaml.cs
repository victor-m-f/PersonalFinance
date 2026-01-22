using System.Linq;
using System.Windows.Controls;
using PersonalFinance.Ui.Settings;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Views.Pages.Settings;

public partial class SettingsPage : Page
{
    private bool _isInitializing;
    private readonly AppSettingsStore _settingsStore = new();
    private AppSettings _settings = new();

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _isInitializing = true;
        _settings = _settingsStore.LoadOrDefault();

        var currentTag = _settings.Theme switch
        {
            AppThemePreference.Dark => "Dark",
            AppThemePreference.Light => "Light",
            _ => "System"
        };

        if (ThemeComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => (string?)item.Tag == currentTag) is
            ComboBoxItem selectedItem)
        {
            ThemeComboBox.SelectedItem = selectedItem;
        }

        _isInitializing = false;
    }

    private void ThemeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
        {
            return;
        }

        if (ThemeComboBox.SelectedItem is not ComboBoxItem selectedItem)
        {
            return;
        }

        switch (selectedItem.Tag as string)
        {
            case "Dark":
                _settings.Theme = AppThemePreference.Dark;
                break;
            case "Light":
                _settings.Theme = AppThemePreference.Light;
                break;
            case "System":
                _settings.Theme = AppThemePreference.System;
                break;
        }

        ThemeApplier.Apply(_settings);

        _settingsStore.Save(_settings);
    }
}

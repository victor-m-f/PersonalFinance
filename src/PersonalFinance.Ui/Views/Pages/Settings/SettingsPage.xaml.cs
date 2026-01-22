using System.Linq;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Views.Pages.Settings;

public partial class SettingsPage : Page
{
    private bool _isInitializing;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _isInitializing = true;
        var currentTheme = ApplicationThemeManager.GetAppTheme();
        var currentTag = currentTheme == ApplicationTheme.Dark ? "Dark" : "Light";

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
                ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.None);
                break;
            case "Light":
                ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.None);
                break;
        }
    }
}

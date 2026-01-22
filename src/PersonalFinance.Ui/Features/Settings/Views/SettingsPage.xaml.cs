using System.Windows.Controls;
using PersonalFinance.Ui.Features.Settings.ViewModels;

namespace PersonalFinance.Ui.Features.Settings.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsPageViewModel();
    }
}

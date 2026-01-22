using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Settings.ViewModels;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.Settings.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<SettingsPageViewModel>();
    }
}

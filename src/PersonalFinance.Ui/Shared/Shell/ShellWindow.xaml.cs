using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Shared.Shell.Services;
using PersonalFinance.Ui.Shared.Shell.ViewModels;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Shared.Shell;

public partial class ShellWindow : FluentWindow
{
    public ShellWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ShellViewModel>();
        ShellNavigationService.Instance.SetNavigationView(NavigationView);
    }
}

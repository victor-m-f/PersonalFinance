using Wpf.Ui.Controls;
using PersonalFinance.Ui.Shared.Shell.Services;

namespace PersonalFinance.Ui.Shared.Shell;

public partial class ShellWindow : FluentWindow
{
    public ShellWindow()
    {
        InitializeComponent();
        ShellNavigationService.Instance.SetNavigationView(NavigationView);
    }
}

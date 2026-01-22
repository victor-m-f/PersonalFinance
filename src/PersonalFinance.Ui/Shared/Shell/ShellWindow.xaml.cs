using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Shared.Shell.Services;
using PersonalFinance.Ui.Shared.Shell.ViewModels;
using Wpf.Ui.Controls;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace PersonalFinance.Ui.Shared.Shell;

public partial class ShellWindow : FluentWindow
{
    public ShellWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ShellViewModel>();
        var pageProvider = App.Services.GetRequiredService<INavigationViewPageProvider>();
        NavigationView.SetPageProviderService(pageProvider);
        var snackbarService = App.Services.GetRequiredService<ISnackbarService>();
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        ShellNavigationService.Instance.SetNavigationView(NavigationView);
    }
}

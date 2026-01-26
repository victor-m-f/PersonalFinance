using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Dashboard.ViewModels;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.Dashboard.Views;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<DashboardPageViewModel>();
    }
}

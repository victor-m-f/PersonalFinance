using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Expenses.ViewModels;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.Expenses.Views;

public partial class ExpensesPage : Page
{
    public ExpensesPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ExpensesPageViewModel>();
    }
}

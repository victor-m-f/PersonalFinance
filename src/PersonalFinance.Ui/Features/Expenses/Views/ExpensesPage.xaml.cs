using System.Windows.Controls;
using PersonalFinance.Ui.Features.Expenses.ViewModels;

namespace PersonalFinance.Ui.Features.Expenses.Views;

public partial class ExpensesPage : Page
{
    public ExpensesPage()
    {
        InitializeComponent();
        DataContext = new ExpensesPageViewModel();
    }
}

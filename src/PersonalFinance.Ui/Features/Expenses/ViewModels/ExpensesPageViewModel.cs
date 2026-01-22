using System.Windows.Input;
using PersonalFinance.Ui.Features.Common.Commands;
using PersonalFinance.Ui.Features.Common.ViewModels;

namespace PersonalFinance.Ui.Features.Expenses.ViewModels;

public sealed class ExpensesPageViewModel : ObservableObject
{
    public ExpensesPageViewModel()
    {
        AddExpenseCommand = new RelayCommand(OnAddExpense);
    }

    public ICommand AddExpenseCommand { get; }

    private void OnAddExpense()
    {
    }
}

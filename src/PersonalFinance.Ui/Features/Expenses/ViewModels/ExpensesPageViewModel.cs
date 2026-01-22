using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

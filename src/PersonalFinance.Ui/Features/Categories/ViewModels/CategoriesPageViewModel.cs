using System.Windows.Input;
using PersonalFinance.Ui.Features.Common.Commands;
using PersonalFinance.Ui.Features.Common.ViewModels;

namespace PersonalFinance.Ui.Features.Categories.ViewModels;

public sealed class CategoriesPageViewModel : ObservableObject
{
    public CategoriesPageViewModel()
    {
        AddCategoryCommand = new RelayCommand(OnAddCategory);
    }

    public ICommand AddCategoryCommand { get; }

    private void OnAddCategory()
    {
    }
}

using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

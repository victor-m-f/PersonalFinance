using System.Windows.Controls;
using PersonalFinance.Ui.Features.Categories.ViewModels;

namespace PersonalFinance.Ui.Features.Categories.Views;

public partial class CategoriesPage : Page
{
    public CategoriesPage()
    {
        InitializeComponent();
        DataContext = new CategoriesPageViewModel();
    }
}

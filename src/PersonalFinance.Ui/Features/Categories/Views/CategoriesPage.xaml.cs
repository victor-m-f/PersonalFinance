using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Categories.ViewModels;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.Categories.Views;

public partial class CategoriesPage : Page
{
    public CategoriesPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<CategoriesPageViewModel>();
    }
}

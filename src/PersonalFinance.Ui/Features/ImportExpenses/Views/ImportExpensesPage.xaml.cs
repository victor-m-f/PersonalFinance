using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.ImportExpenses.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.ImportExpenses.Views;

public partial class ImportExpensesPage : Page
{
    public ImportExpensesPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ImportExpensesPageViewModel>();
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not ImportExpensesPageViewModel viewModel)
        {
            return;
        }

        viewModel.IsDragOver = false;

        if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            await viewModel.HandleDropAsync(files[0]);
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            if (DataContext is ImportExpensesPageViewModel viewModel)
            {
                viewModel.IsDragOver = true;
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
            if (DataContext is ImportExpensesPageViewModel viewModel)
            {
                viewModel.IsDragOver = false;
            }
        }

        e.Handled = true;
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (DataContext is not ImportExpensesPageViewModel viewModel)
        {
            return;
        }

        viewModel.IsDragOver = e.Data.GetDataPresent(DataFormats.FileDrop);
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (DataContext is not ImportExpensesPageViewModel viewModel)
        {
            return;
        }

        viewModel.IsDragOver = false;
    }
}

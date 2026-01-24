using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Categories.Models;
using PersonalFinance.Ui.Features.Categories.ViewModels;
using PersonalFinance.Ui.Features.Categories.Views.Dialogs;
using PersonalFinance.Ui.Features.Expenses.Models;
using PersonalFinance.Ui.Features.Expenses.ViewModels;
using PersonalFinance.Ui.Features.Expenses.Views.Dialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace PersonalFinance.Ui.Features.Categories.Services;

public sealed class UiDialogService : IUiDialogService
{
    private readonly IContentDialogService _contentDialogService;
    private readonly IServiceProvider _serviceProvider;

    public UiDialogService(IContentDialogService contentDialogService, IServiceProvider serviceProvider)
    {
        _contentDialogService = contentDialogService;
        _serviceProvider = serviceProvider;
    }

    public async Task<DialogResult<CategoryEditorResult>> ShowCategoryEditorAsync(
        CategoryEditorDialogOptions options,
        CancellationToken ct)
    {
        var viewModel = _serviceProvider.GetRequiredService<CategoryEditorDialogViewModel>();
        viewModel.Initialize(options);

        var dialog = _serviceProvider.GetRequiredService<CategoryEditorDialog>();
        dialog.DataContext = viewModel;

        var dialogResult = await _contentDialogService.ShowAsync(dialog, ct);
        if (dialogResult != ContentDialogResult.Primary)
        {
            return DialogResult<CategoryEditorResult>.Cancelled();
        }

        var result = viewModel.BuildResult();
        return DialogResult<CategoryEditorResult>.Confirmed(result);
    }

    public async Task<DialogResult<ExpenseEditorResult>> ShowExpenseEditorAsync(
        ExpenseEditorDialogOptions options,
        CancellationToken ct)
    {
        var viewModel = _serviceProvider.GetRequiredService<ExpenseEditorDialogViewModel>();
        viewModel.Initialize(options);

        var dialog = _serviceProvider.GetRequiredService<ExpenseEditorDialog>();
        dialog.DataContext = viewModel;

        var dialogResult = await _contentDialogService.ShowAsync(dialog, ct);
        if (dialogResult != ContentDialogResult.Primary)
        {
            return DialogResult<ExpenseEditorResult>.Cancelled();
        }

        var result = viewModel.BuildResult();
        return DialogResult<ExpenseEditorResult>.Confirmed(result);
    }

    public async Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        CancellationToken ct)
    {
        var result = await _contentDialogService.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions
            {
                Title = title,
                Content = message,
                PrimaryButtonText = confirmText,
                CloseButtonText = cancelText,
                DefaultButton = ContentDialogButton.Primary
            },
            ct);

        return result == ContentDialogResult.Primary;
    }
}

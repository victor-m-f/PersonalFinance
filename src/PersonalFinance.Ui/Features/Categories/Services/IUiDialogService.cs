using PersonalFinance.Ui.Features.Categories.Models;
using PersonalFinance.Ui.Features.Expenses.Models;

namespace PersonalFinance.Ui.Features.Categories.Services;

public interface IUiDialogService
{
    public Task<DialogResult<CategoryEditorResult>> ShowCategoryEditorAsync(
        CategoryEditorDialogOptions options,
        CancellationToken ct);

    public Task<DialogResult<ExpenseEditorResult>> ShowExpenseEditorAsync(
        ExpenseEditorDialogOptions options,
        CancellationToken ct);

    public Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        CancellationToken ct);
}

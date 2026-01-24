using PersonalFinance.Ui.Features.Categories.Models;

namespace PersonalFinance.Ui.Features.Categories.Services;

public interface IUiDialogService
{
    Task<DialogResult<CategoryEditorResult>> ShowCategoryEditorAsync(
        CategoryEditorDialogOptions options,
        CancellationToken ct);

    Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        CancellationToken ct);
}

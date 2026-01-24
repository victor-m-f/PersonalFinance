namespace PersonalFinance.Ui.Features.Categories.Models;

public sealed record class DialogResult<T>
{
    public bool IsConfirmed { get; init; }
    public T? Value { get; init; }

    public static DialogResult<T> Cancelled() => new() { IsConfirmed = false };

    public static DialogResult<T> Confirmed(T value) => new() { IsConfirmed = true, Value = value };
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalFinance.Ui.Features.ImportExpenses.Models;

public sealed class ImportSetupItemVm : ObservableObject
{
    private int _progress;
    private string _statusText = string.Empty;
    private bool _isInstalled;

    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? LanguageCode { get; init; }
    public string? ModelName { get; init; }
    public string? StatusCode { get; init; }
    public bool IsRequired { get; init; }

    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}

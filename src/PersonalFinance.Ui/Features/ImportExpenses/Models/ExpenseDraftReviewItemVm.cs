using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalFinance.Ui.Features.ImportExpenses.Models;

public sealed class ExpenseDraftReviewItemVm : ObservableObject
{
    private Guid? _selectedCategoryId;
    private decimal _amountValue;
    private string _amountText = string.Empty;
    private double _confidenceValue;
    private DateTime _dateValue;
    private string _description = string.Empty;

    public Guid? SuggestedCategoryId { get; init; }
    public string DateText => DateValue.ToString("d", CultureInfo.CurrentCulture);

    public DateTime DateValue
    {
        get => _dateValue;
        set
        {
            if (SetProperty(ref _dateValue, value))
            {
                OnPropertyChanged(nameof(DateText));
            }
        }
    }
    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
            {
                if (TryParseAmount(value, out var parsed))
                {
                    AmountValue = parsed;
                }
            }
        }
    }

    public decimal AmountValue
    {
        get => _amountValue;
        set
        {
            if (SetProperty(ref _amountValue, value))
            {
                var formatted = value.ToString("C", CultureInfo.CurrentCulture);
                if (!string.Equals(_amountText, formatted, StringComparison.Ordinal))
                {
                    _amountText = formatted;
                    OnPropertyChanged(nameof(AmountText));
                }
            }
        }
    }
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    public string ConfidenceText { get; private set; } = string.Empty;

    public double ConfidenceValue
    {
        get => _confidenceValue;
        set
        {
            if (SetProperty(ref _confidenceValue, value))
            {
                ConfidenceText = value.ToString("P0", CultureInfo.CurrentCulture);
                OnPropertyChanged(nameof(ConfidenceText));
                OnPropertyChanged(nameof(IsConfidenceLow));
            }
        }
    }

    public bool IsConfidenceLow => ConfidenceValue < 0.75d;

    public Guid? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set => SetProperty(ref _selectedCategoryId, value);
    }

    private static bool TryParseAmount(string? text, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim();
        normalized = normalized.Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, string.Empty).Trim();

        return decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out value);
    }
}

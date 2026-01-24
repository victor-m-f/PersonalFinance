using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Shared.Constraints;
using PersonalFinance.Ui.Features.Categories.Models;
using PersonalFinance.Ui.Features.Expenses.Models;
using PersonalFinance.Ui.Shared.Search;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Features.Expenses.ViewModels;

public sealed class ExpenseEditorDialogViewModel : ObservableObject, IDataErrorInfo
{
    private Guid? _id;
    private DateTime? _date;
    private decimal? _amount;
    private string _amountText = string.Empty;
    private string _description = string.Empty;
    private Guid? _categoryId;
    private bool _isEditMode;
    private IReadOnlyList<CategoryLookupItemVm> _categoryOptions = Array.Empty<CategoryLookupItemVm>();
    private IReadOnlyList<CategoryLookupItemVm> _filteredCategoryOptions = Array.Empty<CategoryLookupItemVm>();
    private bool _isValid;
    private bool _suppressValidation;
    private bool _isDateTouched;
    private bool _isAmountTouched;
    private bool _isDescriptionTouched;
    private string _categorySearchText = string.Empty;

    public IRelayCommand DateLostFocusCommand { get; }
    public IRelayCommand AmountLostFocusCommand { get; }
    public IRelayCommand DescriptionLostFocusCommand { get; }
    public IRelayCommand<AutoSuggestBoxSuggestionChosenEventArgs> CategorySuggestionChosenCommand { get; }
    public IRelayCommand<AutoSuggestBoxTextChangedEventArgs> CategoryTextChangedCommand { get; }

    public Guid? Id
    {
        get => _id;
        private set => SetProperty(ref _id, value);
    }

    public DateTime? Date
    {
        get => _date;
        set
        {
            if (SetProperty(ref _date, value))
            {
                if (!_suppressValidation)
                {
                    _isDateTouched = true;
                }

                UpdateValidationState();
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
                if (!_suppressValidation)
                {
                    _isAmountTouched = true;
                }

                _amount = ParseAmount(_amountText);
                UpdateValidationState();
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                if (!_suppressValidation)
                {
                    _isDescriptionTouched = true;
                }

                UpdateValidationState();
            }
        }
    }

    public Guid? CategoryId
    {
        get => _categoryId;
        set => SetProperty(ref _categoryId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set => SetProperty(ref _isEditMode, value);
    }

    public IReadOnlyList<CategoryLookupItemVm> CategoryOptions
    {
        get => _categoryOptions;
        private set => SetProperty(ref _categoryOptions, value);
    }

    public IReadOnlyList<CategoryLookupItemVm> FilteredCategoryOptions
    {
        get => _filteredCategoryOptions;
        private set => SetProperty(ref _filteredCategoryOptions, value);
    }

    public string CategorySearchText
    {
        get => _categorySearchText;
        set
        {
            if (SetProperty(ref _categorySearchText, value))
            {
                if (!_suppressValidation && string.IsNullOrWhiteSpace(_categorySearchText))
                {
                    CategoryId = null;
                }
            }
        }
    }

    public bool IsValid
    {
        get => _isValid;
        private set => SetProperty(ref _isValid, value);
    }

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Date) => _isDateTouched ? ValidateDate() : string.Empty,
        nameof(AmountText) => _isAmountTouched ? ValidateAmount() : string.Empty,
        nameof(Description) => _isDescriptionTouched ? ValidateDescription() : string.Empty,
        _ => string.Empty
    };

    public ExpenseEditorDialogViewModel()
    {
        DateLostFocusCommand = new RelayCommand(OnDateLostFocus);
        AmountLostFocusCommand = new RelayCommand(OnAmountLostFocus);
        DescriptionLostFocusCommand = new RelayCommand(OnDescriptionLostFocus);
        CategorySuggestionChosenCommand = new RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs>(OnCategorySuggestionChosen);
        CategoryTextChangedCommand = new RelayCommand<AutoSuggestBoxTextChangedEventArgs>(OnCategoryTextChanged);
    }

    public void Initialize(ExpenseEditorDialogOptions options)
    {
        _suppressValidation = true;

        Id = options.Id;
        IsEditMode = options.IsEditMode;
        Date = options.Date == default ? DateTime.Today : options.Date;
        AmountText = options.Amount.ToString("0.##", CultureInfo.CurrentCulture);
        Description = options.Description ?? string.Empty;
        CategoryId = options.CategoryId;
        CategoryOptions = options.CategoryOptions;
        FilteredCategoryOptions = options.CategoryOptions;
        CategorySearchText = ResolveCategorySearchText(options.CategoryId, options.CategoryOptions);

        _isDateTouched = false;
        _isAmountTouched = false;
        _isDescriptionTouched = false;
        _suppressValidation = false;
        UpdateValidationState();
    }

    public ExpenseEditorResult BuildResult()
    {
        return new ExpenseEditorResult
        {
            Id = Id,
            Date = Date ?? DateTime.Today,
            Amount = _amount ?? 0m,
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            CategoryId = string.IsNullOrWhiteSpace(CategorySearchText) ? null : CategoryId,
            IsEditMode = IsEditMode
        };
    }

    private void UpdateValidationState()
    {
        IsValid = string.IsNullOrEmpty(ValidateDate())
            && string.IsNullOrEmpty(ValidateAmount())
            && string.IsNullOrEmpty(ValidateDescription());
    }

    private void OnDateLostFocus()
    {
        _isDateTouched = true;
        OnPropertyChanged(nameof(Date));
        UpdateValidationState();
    }

    private void OnAmountLostFocus()
    {
        _isAmountTouched = true;
        OnPropertyChanged(nameof(AmountText));
        UpdateValidationState();
    }

    private void OnDescriptionLostFocus()
    {
        _isDescriptionTouched = true;
        OnPropertyChanged(nameof(Description));
        UpdateValidationState();
    }

    private void OnCategorySuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs? args)
    {
        if (args?.SelectedItem is not CategoryLookupItemVm option)
        {
            return;
        }

        CategoryId = option.Id;
        CategorySearchText = option.DisplayName;
        FilteredCategoryOptions = CategoryOptions;
    }

    private void OnCategoryTextChanged(AutoSuggestBoxTextChangedEventArgs? args)
    {
        if (args is null)
        {
            return;
        }

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            args.Handled = true;
            FilteredCategoryOptions = FilterOptions(CategoryOptions, args.Text);
        }

        if (string.IsNullOrWhiteSpace(args.Text))
        {
            CategoryId = null;
            FilteredCategoryOptions = CategoryOptions;
        }
    }

    private static IReadOnlyList<CategoryLookupItemVm> FilterOptions(
        IReadOnlyList<CategoryLookupItemVm> options,
        string? searchText)
    {
        var normalized = TextSearchNormalizer.Normalize(searchText);
        if (normalized is null)
        {
            return options;
        }

        return options
            .Where(option => TextSearchNormalizer.Normalize(option.DisplayName)?.Contains(normalized, StringComparison.Ordinal) == true)
            .ToList();
    }

    private static string ResolveCategorySearchText(Guid? categoryId, IReadOnlyList<CategoryLookupItemVm> options)
    {
        if (categoryId is null)
        {
            return string.Empty;
        }

        var selected = options.FirstOrDefault(option => option.Id == categoryId);
        return selected?.DisplayName ?? string.Empty;
    }

    private static decimal? ParseAmount(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount))
        {
            return amount;
        }

        return null;
    }

    private string ValidateDate()
    {
        if (!Date.HasValue || Date.Value == default)
        {
            return "Date is required.";
        }

        return string.Empty;
    }

    private string ValidateAmount()
    {
        if (_amount is null)
        {
            return "Amount is required.";
        }

        if (_amount <= 0)
        {
            return "Amount must be greater than zero.";
        }

        return string.Empty;
    }

    private string ValidateDescription()
    {
        var trimmed = Description?.Trim() ?? string.Empty;
        if (trimmed.Length > EntityConstraints.Expense.DescriptionMaxLength)
        {
            return $"Description must be {EntityConstraints.Expense.DescriptionMaxLength} characters or less.";
        }

        return string.Empty;
    }
}

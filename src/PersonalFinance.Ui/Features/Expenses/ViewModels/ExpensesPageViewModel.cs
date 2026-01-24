using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.Responses;
using PersonalFinance.Application.Expenses.UseCases;
using PersonalFinance.Shared.Results;
using PersonalFinance.Ui.Features.Categories.Models;
using PersonalFinance.Ui.Features.Categories.Services;
using PersonalFinance.Ui.Features.Expenses.Models;
using PersonalFinance.Ui.Shared.Collections;
using PersonalFinance.Ui.Shared.Search;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Features.Expenses.ViewModels;

public sealed class ExpensesPageViewModel : ObservableObject
{
    private const int DefaultPageSize = 15;

    private readonly FilterExpensesUseCase _filterExpensesUseCase;
    private readonly CreateExpenseUseCase _createExpenseUseCase;
    private readonly UpdateExpenseUseCase _updateExpenseUseCase;
    private readonly DeleteExpenseUseCase _deleteExpenseUseCase;
    private readonly AssignExpenseCategoryUseCase _assignExpenseCategoryUseCase;
    private readonly FilterCategoriesUseCase _filterCategoriesUseCase;
    private readonly IUiDialogService _dialogService;
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<ExpensesPageViewModel> _logger;
    private readonly DispatcherTimer _searchDebounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadRequestId;
    private IReadOnlyList<CategoryLookupItemVm>? _categoryOptionsCache;
    private bool _isCategoryOptionsCacheDirty = true;

    private string _searchText = string.Empty;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private string _minAmountText = string.Empty;
    private string _maxAmountText = string.Empty;
    private Guid? _categoryId;
    private string _categorySearchText = string.Empty;
    private IReadOnlyList<CategoryLookupItemVm> _categoryOptions = Array.Empty<CategoryLookupItemVm>();
    private IReadOnlyList<CategoryLookupItemVm> _filteredCategoryOptions = Array.Empty<CategoryLookupItemVm>();
    private int _pageNumber = 1;
    private int _totalCount;
    private int _pageCount;
    private bool _isBusy;
    private bool _isInitialized;

    public ObservableCollectionEx<ExpenseListItemVm> Items { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                StartSearchDebounce();
            }
        }
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                OnFilterChanged();
            }
        }
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set
        {
            if (SetProperty(ref _endDate, value))
            {
                OnFilterChanged();
            }
        }
    }

    public string MinAmountText
    {
        get => _minAmountText;
        set
        {
            if (SetProperty(ref _minAmountText, value))
            {
                OnFilterChanged();
            }
        }
    }

    public string MaxAmountText
    {
        get => _maxAmountText;
        set
        {
            if (SetProperty(ref _maxAmountText, value))
            {
                OnFilterChanged();
            }
        }
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
        set => SetProperty(ref _categorySearchText, value);
    }

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (SetProperty(ref _pageNumber, value))
            {
                OnPropertyChanged(nameof(PageText));
                UpdatePagingCommands();
            }
        }
    }

    public static int PageSize => DefaultPageSize;

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(ShowingText));
            }
        }
    }

    public int PageCount
    {
        get => _pageCount;
        private set
        {
            if (SetProperty(ref _pageCount, value))
            {
                OnPropertyChanged(nameof(PageText));
                UpdatePagingCommands();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                UpdatePagingCommands();
                LoadCommand.NotifyCanExecuteChanged();
                NewCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ShowingText => $"{GetString("Expenses.Showing", "Showing")} {Items.Count} {GetString("Expenses.Of", "of")} {TotalCount}";

    public string PageText => PageCount == 0
        ? $"{GetString("Expenses.Page", "Page")} 0 {GetString("Expenses.Of", "of")} 0"
        : $"{GetString("Expenses.Page", "Page")} {PageNumber} {GetString("Expenses.Of", "of")} {PageCount}";

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand NewCommand { get; }
    public IAsyncRelayCommand<ExpenseListItemVm?> EditCommand { get; }
    public IAsyncRelayCommand<ExpenseListItemVm?> DeleteCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PrevPageCommand { get; }
    public IAsyncRelayCommand<AutoSuggestBoxSuggestionChosenEventArgs> CategorySuggestionChosenCommand { get; }
    public IAsyncRelayCommand<AutoSuggestBoxTextChangedEventArgs> CategoryTextChangedCommand { get; }

    public ExpensesPageViewModel(
        FilterExpensesUseCase filterExpensesUseCase,
        CreateExpenseUseCase createExpenseUseCase,
        UpdateExpenseUseCase updateExpenseUseCase,
        DeleteExpenseUseCase deleteExpenseUseCase,
        AssignExpenseCategoryUseCase assignExpenseCategoryUseCase,
        FilterCategoriesUseCase filterCategoriesUseCase,
        IUiDialogService dialogService,
        ISnackbarService snackbarService,
        ILogger<ExpensesPageViewModel> logger)
    {
        _filterExpensesUseCase = filterExpensesUseCase;
        _createExpenseUseCase = createExpenseUseCase;
        _updateExpenseUseCase = updateExpenseUseCase;
        _deleteExpenseUseCase = deleteExpenseUseCase;
        _assignExpenseCategoryUseCase = assignExpenseCategoryUseCase;
        _filterCategoriesUseCase = filterCategoriesUseCase;
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        _logger = logger;

        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        InitializeCommand = new AsyncRelayCommand(InitializeAsync, () => !_isInitialized);
        NewCommand = new AsyncRelayCommand(CreateAsync, () => !IsBusy);
        EditCommand = new AsyncRelayCommand<ExpenseListItemVm?>(EditAsync, _ => !IsBusy);
        DeleteCommand = new AsyncRelayCommand<ExpenseListItemVm?>(DeleteAsync, _ => !IsBusy);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync, CanGoNext);
        PrevPageCommand = new AsyncRelayCommand(PrevPageAsync, CanGoPrev);
        CategorySuggestionChosenCommand = new AsyncRelayCommand<AutoSuggestBoxSuggestionChosenEventArgs>(OnCategorySuggestionChosenAsync);
        CategoryTextChangedCommand = new AsyncRelayCommand<AutoSuggestBoxTextChangedEventArgs>(OnCategoryTextChangedAsync);

        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _searchDebounceTimer.Tick += OnSearchDebounceTick;
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        InitializeCommand.NotifyCanExecuteChanged();

        await Dispatcher.Yield(DispatcherPriority.Background);
        await LoadCategoryOptionsAsync(CancellationToken.None);
        await LoadAsync();
    }

    private void StartSearchDebounce()
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private void OnSearchDebounceTick(object? sender, EventArgs e)
    {
        _searchDebounceTimer.Stop();
        PageNumber = 1;
        _ = RequestLoadAsync();
    }

    private void OnFilterChanged()
    {
        if (!_isInitialized)
        {
            return;
        }

        PageNumber = 1;
        _ = RequestLoadAsync();
    }

    private Task OnCategorySuggestionChosenAsync(AutoSuggestBoxSuggestionChosenEventArgs? args)
    {
        if (args?.SelectedItem is not CategoryLookupItemVm option)
        {
            return Task.CompletedTask;
        }

        _categoryId = option.Id;
        CategorySearchText = option.DisplayName;
        FilteredCategoryOptions = CategoryOptions;
        OnFilterChanged();
        return Task.CompletedTask;
    }

    private Task OnCategoryTextChangedAsync(AutoSuggestBoxTextChangedEventArgs? args)
    {
        if (args is null)
        {
            return Task.CompletedTask;
        }

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            args.Handled = true;
            FilteredCategoryOptions = FilterOptions(CategoryOptions, args.Text);
        }

        if (string.IsNullOrWhiteSpace(args.Text))
        {
            _categoryId = null;
            FilteredCategoryOptions = CategoryOptions;
            OnFilterChanged();
        }

        return Task.CompletedTask;
    }

    private async Task RequestLoadAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var requestId = Interlocked.Increment(ref _loadRequestId);
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        IsBusy = true;
        try
        {
            var request = new FilterExpensesRequest
            {
                StartDate = NormalizeStartDate(StartDate),
                EndDate = NormalizeEndDate(EndDate),
                MinAmount = ParseAmountOrNull(MinAmountText),
                MaxAmount = ParseAmountOrNull(MaxAmountText),
                CategoryId = _categoryId,
                DescriptionSearch = SearchText,
                SortBy = "Date",
                SortDescending = true,
                Page = new PageRequest { PageNumber = PageNumber, PageSize = PageSize }
            };

            var result = await _filterExpensesUseCase.ExecuteAsync(request, ct);
            if (ct.IsCancellationRequested || requestId != _loadRequestId)
            {
                return;
            }

            if (!result.IsSuccess)
            {
                ShowError(result, "Failed to load expenses.");
                return;
            }

            await ApplyResultAsync(result.Value!, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (requestId == _loadRequestId)
            {
                _logger.LogError(ex, "Failed to load expenses.");
                _snackbarService.Show("Error", "Failed to load expenses.", ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
            }
        }
        finally
        {
            if (requestId == _loadRequestId)
            {
                IsBusy = false;
            }
        }
    }

    private async Task CreateAsync()
    {
        if (IsBusy)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var categoryOptions = await LoadCategoryOptionsAsync(cts.Token);

        var dialogResult = await _dialogService.ShowExpenseEditorAsync(new ExpenseEditorDialogOptions
        {
            Id = null,
            Date = DateTime.Today,
            Amount = 0m,
            Description = string.Empty,
            CategoryId = null,
            IsEditMode = false,
            CategoryOptions = categoryOptions
        }, cts.Token);

        if (!dialogResult.IsConfirmed || dialogResult.Value is null)
        {
            return;
        }

        var request = new CreateExpenseRequest
        {
            Date = dialogResult.Value.Date,
            Amount = dialogResult.Value.Amount,
            Description = dialogResult.Value.Description,
            CategoryId = dialogResult.Value.CategoryId
        };

        var result = await ExecuteWithBusyAsync(ct => _createExpenseUseCase.ExecuteAsync(request, ct));
        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to create expense.");
            return;
        }

        _snackbarService.Show("Success", "Expense created.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        await LoadAsync();
    }

    private async Task EditAsync(ExpenseListItemVm? item)
    {
        if (item is null || IsBusy)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var categoryOptions = await LoadCategoryOptionsAsync(cts.Token);

        var dialogResult = await _dialogService.ShowExpenseEditorAsync(new ExpenseEditorDialogOptions
        {
            Id = item.Id,
            Date = item.Date,
            Amount = item.Amount,
            Description = item.Description,
            CategoryId = item.CategoryId,
            IsEditMode = true,
            CategoryOptions = categoryOptions
        }, cts.Token);

        if (!dialogResult.IsConfirmed || dialogResult.Value is null)
        {
            return;
        }

        var request = new UpdateExpenseRequest
        {
            Id = item.Id,
            Date = dialogResult.Value.Date,
            Amount = dialogResult.Value.Amount,
            Description = dialogResult.Value.Description
        };

        var result = await ExecuteWithBusyAsync(ct => _updateExpenseUseCase.ExecuteAsync(request, ct));
        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to update expense.");
            return;
        }

        if (dialogResult.Value.CategoryId != item.CategoryId)
        {
            var assignResult = await ExecuteWithBusyAsync(ct => _assignExpenseCategoryUseCase.ExecuteAsync(new AssignExpenseCategoryRequest
            {
                Id = item.Id,
                CategoryId = dialogResult.Value.CategoryId
            }, ct));
            if (assignResult is null)
            {
                return;
            }

            if (!assignResult.IsSuccess)
            {
                ShowError(assignResult, "Failed to update expense category.");
                return;
            }
        }

        _snackbarService.Show("Success", "Expense updated.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        await LoadAsync();
    }

    private async Task DeleteAsync(ExpenseListItemVm? item)
    {
        if (item is null || IsBusy)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var confirmed = await _dialogService.ShowConfirmAsync(
            GetString("Expenses.DeleteConfirmTitle", "Delete expense"),
            GetString("Expenses.DeleteConfirmMessage", "Are you sure you want to delete this expense?"),
            GetString("Common.Delete", "Delete"),
            GetString("Common.Cancel", "Cancel"),
            cts.Token);

        if (!confirmed)
        {
            return;
        }

        var result = await ExecuteWithBusyAsync(ct => _deleteExpenseUseCase.ExecuteAsync(new DeleteExpenseRequest
        {
            Id = item.Id
        }, ct));
        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to delete expense.");
            return;
        }

        _snackbarService.Show("Success", "Expense deleted.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        await LoadAsync();
    }

    private async Task NextPageAsync()
    {
        if (!CanGoNext())
        {
            return;
        }

        PageNumber++;
        await LoadAsync();
    }

    private async Task PrevPageAsync()
    {
        if (!CanGoPrev())
        {
            return;
        }

        PageNumber--;
        await LoadAsync();
    }

    private bool CanGoNext() => !IsBusy && PageNumber < PageCount;

    private bool CanGoPrev() => !IsBusy && PageNumber > 1;

    private void UpdatePagingCommands()
    {
        NextPageCommand.NotifyCanExecuteChanged();
        PrevPageCommand.NotifyCanExecuteChanged();
    }

    private async Task<IReadOnlyList<CategoryLookupItemVm>> LoadCategoryOptionsAsync(CancellationToken ct)
    {
        var cached = _categoryOptionsCache;
        if (!_isCategoryOptionsCacheDirty && cached is not null)
        {
            CategoryOptions = cached;
            FilteredCategoryOptions = cached;
            return cached;
        }

        var request = new FilterCategoriesRequest
        {
            IncludeAll = true,
            SortBy = "Name",
            SortDescending = false,
            Page = null,
            ParentId = null
        };

        var result = await _filterCategoriesUseCase.ExecuteAsync(request, ct);
        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to load categories.");
            CategoryOptions = Array.Empty<CategoryLookupItemVm>();
            FilteredCategoryOptions = Array.Empty<CategoryLookupItemVm>();
            return Array.Empty<CategoryLookupItemVm>();
        }

        var items = result.Value!.Items.ToList();
        var parentMap = items.ToDictionary(item => item.Id, item => item.ParentId);
        var depthMap = BuildDepthMap(parentMap);

        var lookup = items
            .OrderBy(item => item.Name)
            .Select(item => new CategoryLookupItemVm
            {
                Id = item.Id,
                Name = item.Name,
                Depth = depthMap.TryGetValue(item.Id, out var depth) ? depth : 0,
                DisplayName = BuildDisplayName(item.Name, depthMap.TryGetValue(item.Id, out var resolvedDepth) ? resolvedDepth : 0)
            })
            .ToList();

        _categoryOptionsCache = lookup;
        _isCategoryOptionsCacheDirty = false;
        CategoryOptions = lookup;
        FilteredCategoryOptions = lookup;
        return lookup;
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

    private async Task ApplyResultAsync(PagedResult<ExpenseListItemResponse> result, CancellationToken ct)
    {
        var uncategorized = GetString("Expenses.CategoryNone", "Uncategorized");
        var emptyDescription = GetString("Expenses.DescriptionEmpty", "No description");
        var culture = CultureInfo.CurrentCulture;

        var items = await Task.Run(() =>
        {
            var list = new List<ExpenseListItemVm>(result.Items.Count);

            foreach (var item in result.Items)
            {
                var description = string.IsNullOrWhiteSpace(item.Description)
                    ? emptyDescription
                    : item.Description;

                var categoryDisplay = item.CategoryName ?? uncategorized;

                list.Add(new ExpenseListItemVm
                {
                    Id = item.Id,
                    Date = item.Date,
                    Amount = item.Amount,
                    Description = item.Description,
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    CategoryColorHex = item.CategoryColorHex,
                    DateDisplay = item.Date.ToString("d", culture),
                    AmountDisplay = item.Amount.ToString("C", culture),
                    DescriptionDisplay = description,
                    CategoryDisplay = categoryDisplay
                });
            }

            return list;
        }, ct);

        if (ct.IsCancellationRequested)
        {
            return;
        }

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Items.ReplaceRange(items);

            TotalCount = result.TotalCount;
            PageCount = result.PageCount;
            PageNumber = result.PageNumber;
            OnPropertyChanged(nameof(ShowingText));
        }, DispatcherPriority.Background, ct);
    }

    private static Dictionary<Guid, int> BuildDepthMap(Dictionary<Guid, Guid?> parentMap)
    {
        var depthMap = new Dictionary<Guid, int>();

        foreach (var item in parentMap)
        {
            var depth = 0;
            var current = item.Value;
            var guard = new HashSet<Guid>();
            while (current.HasValue && parentMap.TryGetValue(current.Value, out var parentId))
            {
                if (!guard.Add(current.Value))
                {
                    break;
                }

                depth++;
                current = parentId;
            }

            depthMap[item.Key] = depth;
        }

        return depthMap;
    }

    private static string BuildDisplayName(string name, int depth)
    {
        if (depth <= 0)
        {
            return name;
        }

        var prefix = string.Concat(Enumerable.Repeat("— ", depth));
        return $"{prefix}{name}";
    }

    private void ShowError<T>(Result<T> result, string fallbackMessage)
    {
        var message = result.ErrorMessage ?? fallbackMessage;
        _logger.LogWarning("{Message} ({Code})", message, result.ErrorCode ?? "Unknown");
        _snackbarService.Show("Error", message, ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
    }

    private void ShowError(Result result, string fallbackMessage)
    {
        var message = result.ErrorMessage ?? fallbackMessage;
        _logger.LogWarning("{Message} ({Code})", message, result.ErrorCode ?? "Unknown");
        _snackbarService.Show("Error", message, ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
    }

    private static string GetString(string key, string fallback)
    {
        return System.Windows.Application.Current.TryFindResource(key) as string ?? fallback;
    }

    private static DateTime? NormalizeStartDate(DateTime? date)
    {
        return date?.Date;
    }

    private static DateTime? NormalizeEndDate(DateTime? date)
    {
        return date?.Date.AddDays(1).AddTicks(-1);
    }

    private static decimal? ParseAmountOrNull(string? value)
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

    private async Task<Result<T>?> ExecuteWithBusyAsync<T>(Func<CancellationToken, Task<Result<T>>> action)
    {
        using var cts = new CancellationTokenSource();
        IsBusy = true;
        try
        {
            return await action(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Result?> ExecuteWithBusyAsync(Func<CancellationToken, Task<Result>> action)
    {
        using var cts = new CancellationTokenSource();
        IsBusy = true;
        try
        {
            return await action(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

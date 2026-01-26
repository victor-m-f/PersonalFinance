using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.Responses;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Shared.Results;
using PersonalFinance.Ui.Features.Categories.Models;
using PersonalFinance.Ui.Features.Categories.Services;
using PersonalFinance.Ui.Shared.Collections;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Features.Categories.ViewModels;

public sealed class CategoriesPageViewModel : ObservableObject
{
    private const int DefaultPageSize = 15;

    private readonly FilterCategoriesUseCase _filterCategoriesUseCase;
    private readonly CreateCategoryUseCase _createCategoryUseCase;
    private readonly UpdateCategoryUseCase _updateCategoryUseCase;
    private readonly DeleteCategoryUseCase _deleteCategoryUseCase;
    private readonly IUiDialogService _dialogService;
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<CategoriesPageViewModel> _logger;
    private readonly DispatcherTimer _searchDebounceTimer;
    private CancellationTokenSource? _loadCts;
    private int _loadRequestId;
    private IReadOnlyList<CategoryLookupItemVm>? _parentOptionsCache;
    private bool _isParentOptionsCacheDirty = true;

    private string _searchText = string.Empty;
    private int _pageNumber = 1;
    private int _totalCount;
    private int _pageCount;
    private bool _showRootsOnly;
    private bool _isBusy;
    private bool _isInitialized;

    public ObservableCollectionEx<CategoryListItemVm> Items { get; } = [];

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

    public bool ShowRootsOnly
    {
        get => _showRootsOnly;
        set
        {
            if (SetProperty(ref _showRootsOnly, value))
            {
                PageNumber = 1;
                _ = RequestLoadAsync();
            }
        }
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

    public string ShowingText => $"{GetString("Categories.Showing", "Showing")} {Items.Count} {GetString("Categories.Of", "of")} {TotalCount}";

    public string PageText => PageCount == 0
        ? $"{GetString("Categories.Page", "Page")} 0 {GetString("Categories.Of", "of")} 0"
        : $"{GetString("Categories.Page", "Page")} {PageNumber} {GetString("Categories.Of", "of")} {PageCount}";

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand NewCommand { get; }
    public IAsyncRelayCommand<CategoryListItemVm?> EditCommand { get; }
    public IAsyncRelayCommand<CategoryListItemVm?> DeleteCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PrevPageCommand { get; }

    public CategoriesPageViewModel(
        FilterCategoriesUseCase filterCategoriesUseCase,
        CreateCategoryUseCase createCategoryUseCase,
        UpdateCategoryUseCase updateCategoryUseCase,
        DeleteCategoryUseCase deleteCategoryUseCase,
        IUiDialogService dialogService,
        ISnackbarService snackbarService,
        ILogger<CategoriesPageViewModel> logger)
    {
        _filterCategoriesUseCase = filterCategoriesUseCase;
        _createCategoryUseCase = createCategoryUseCase;
        _updateCategoryUseCase = updateCategoryUseCase;
        _deleteCategoryUseCase = deleteCategoryUseCase;
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        _logger = logger;

        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        InitializeCommand = new AsyncRelayCommand(InitializeAsync, () => !_isInitialized);
        NewCommand = new AsyncRelayCommand(CreateAsync, () => !IsBusy);
        EditCommand = new AsyncRelayCommand<CategoryListItemVm?>(EditAsync, _ => !IsBusy);
        DeleteCommand = new AsyncRelayCommand<CategoryListItemVm?>(DeleteAsync, _ => !IsBusy);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync, CanGoNext);
        PrevPageCommand = new AsyncRelayCommand(PrevPageAsync, CanGoPrev);

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
            var request = new FilterCategoriesRequest
            {
                Name = SearchText,
                ParentId = null,
                IncludeAll = !ShowRootsOnly,
                SortBy = "Name",
                SortDescending = false,
                Page = new PageRequest { PageNumber = PageNumber, PageSize = PageSize }
            };

            var result = await _filterCategoriesUseCase.ExecuteAsync(request, ct);
            if (ct.IsCancellationRequested || requestId != _loadRequestId)
            {
                return;
            }

            if (!result.IsSuccess)
            {
                ShowError(result, "Failed to load categories.");
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
                _logger.LogError(ex, "Failed to load categories.");
                _snackbarService.Show("Error", "Failed to load categories.", ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
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
        var parentOptions = await LoadParentOptionsAsync(null, cts.Token);
        var dialogResult = await _dialogService.ShowCategoryEditorAsync(new CategoryEditorDialogOptions
        {
            Id = null,
            Name = string.Empty,
            ColorHex = "#FF3B82F6",
            ParentId = null,
            IsEditMode = false,
            ParentOptions = parentOptions
        }, cts.Token);

        if (!dialogResult.IsConfirmed || dialogResult.Value is null)
        {
            return;
        }

        var request = new CreateCategoryRequest
        {
            Name = dialogResult.Value.Name,
            ColorHex = dialogResult.Value.ColorHex,
            ParentId = dialogResult.Value.ParentId
        };

        var result = await ExecuteWithBusyAsync(ct => _createCategoryUseCase.ExecuteAsync(request, ct));
        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to create category.");
            return;
        }

        _snackbarService.Show("Success", "Category created.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        InvalidateParentOptionsCache();
        await LoadAsync();
    }

    private async Task EditAsync(CategoryListItemVm? item)
    {
        if (item is null || IsBusy)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var parentOptions = await LoadParentOptionsAsync(item.Id, cts.Token);
        var dialogResult = await _dialogService.ShowCategoryEditorAsync(new CategoryEditorDialogOptions
        {
            Id = item.Id,
            Name = item.Name,
            ColorHex = item.ColorHex,
            ParentId = item.ParentId,
            IsEditMode = true,
            ParentOptions = parentOptions
        }, cts.Token);

        if (!dialogResult.IsConfirmed || dialogResult.Value is null)
        {
            return;
        }

        var request = new UpdateCategoryRequest
        {
            Id = item.Id,
            Name = dialogResult.Value.Name,
            ColorHex = dialogResult.Value.ColorHex,
            ParentId = dialogResult.Value.ParentId
        };

        var result = await ExecuteWithBusyAsync(ct => _updateCategoryUseCase.ExecuteAsync(request, ct));
        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to update category.");
            return;
        }

        _snackbarService.Show("Success", "Category updated.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        InvalidateParentOptionsCache();
        await LoadAsync();
    }

    private async Task DeleteAsync(CategoryListItemVm? item)
    {
        if (item is null || IsBusy)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var confirmed = await _dialogService.ShowConfirmAsync(
            GetString("Categories.DeleteConfirmTitle", "Delete category"),
            GetString("Categories.DeleteConfirmMessage", "Are you sure you want to delete this category?"),
            GetString("Common.Delete", "Delete"),
            GetString("Common.Cancel", "Cancel"),
            cts.Token);

        if (!confirmed)
        {
            return;
        }

        var result = await ExecuteWithBusyAsync(ct => _deleteCategoryUseCase.ExecuteAsync(new DeleteCategoryRequest
        {
            Id = item.Id
        }, ct));
        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to delete category.");
            return;
        }

        _snackbarService.Show("Success", "Category deleted.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        InvalidateParentOptionsCache();
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

    private async Task<IReadOnlyList<CategoryLookupItemVm>> LoadParentOptionsAsync(Guid? excludeId, CancellationToken ct)
    {
        var cached = _parentOptionsCache;
        if (!_isParentOptionsCacheDirty && cached is not null)
        {
            return ApplyExclude(cached, excludeId);
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
            ShowError(result, "Failed to load parent categories.");
            return new List<CategoryLookupItemVm>();
        }

        var items = result.Value!.Items
            .Where(item => excludeId == null || item.Id != excludeId)
            .ToList();

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

        _parentOptionsCache = lookup;
        _isParentOptionsCacheDirty = false;

        return ApplyExclude(lookup, excludeId);
    }

    private async Task ApplyResultAsync(PagedResult<CategoryListItemResponse> result, CancellationToken ct)
    {
        var (subcategoryOf, subcategory) = ResolveUiStrings();

        var items = await Task.Run(() =>
        {
            var parentMap = result.Items.ToDictionary(item => item.Id, item => item.ParentId);
            var nameMap = result.Items.ToDictionary(item => item.Id, item => item.Name);
            var depthMap = BuildDepthMap(parentMap);
            var orderedItems = OrderByHierarchy(result.Items, parentMap, nameMap);

            var list = new List<CategoryListItemVm>(orderedItems.Count);

            foreach (var item in orderedItems)
            {
                var parentHint = string.Empty;

                if (item.ParentId.HasValue)
                {
                    if (nameMap.TryGetValue(item.ParentId.Value, out var parentName))
                    {
                        parentHint = $"{subcategoryOf} {parentName}";
                    }
                    else
                    {
                        parentHint = subcategory;
                    }
                }

                list.Add(new CategoryListItemVm
                {
                    Id = item.Id,
                    Name = item.Name,
                    ColorHex = item.ColorHex,
                    ParentId = item.ParentId,
                    Depth = depthMap.TryGetValue(item.Id, out var depth) ? depth : 0,
                    ParentHint = parentHint
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

    private static (string SubcategoryOf, string Subcategory) ResolveUiStrings()
    {
        return (
            GetString("Categories.SubcategoryOf", "Subcategory of"),
            GetString("Categories.Subcategory", "Subcategory")
        );
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

    private static List<CategoryListItemResponse> OrderByHierarchy(
        IReadOnlyList<CategoryListItemResponse> items,
        Dictionary<Guid, Guid?> parentMap,
        Dictionary<Guid, string> nameMap)
    {
        var childrenMap = new Dictionary<Guid, List<CategoryListItemResponse>>();

        foreach (var item in items)
        {
            var key = item.ParentId ?? Guid.Empty;
            if (!childrenMap.TryGetValue(key, out var list))
            {
                list = new List<CategoryListItemResponse>();
                childrenMap[key] = list;
            }

            list.Add(item);
        }

        foreach (var list in childrenMap.Values)
        {
            list.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        var ordered = new List<CategoryListItemResponse>(items.Count);
        var visited = new HashSet<Guid>();

        void Visit(Guid? parentId)
        {
            var key = parentId ?? Guid.Empty;
            if (!childrenMap.TryGetValue(key, out var children))
            {
                return;
            }

            foreach (var child in children)
            {
                if (!visited.Add(child.Id))
                {
                    continue;
                }

                ordered.Add(child);
                Visit(child.Id);
            }
        }

        Visit(null);

        if (ordered.Count < items.Count)
        {
            var remaining = items.Where(item => !visited.Contains(item.Id))
                .OrderBy(item => nameMap.TryGetValue(item.Id, out var name) ? name : item.Name)
                .ToList();

            ordered.AddRange(remaining);
        }

        return ordered;
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

    private void InvalidateParentOptionsCache()
    {
        _isParentOptionsCacheDirty = true;
    }

    private static IReadOnlyList<CategoryLookupItemVm> ApplyExclude(
        IReadOnlyList<CategoryLookupItemVm> source,
        Guid? excludeId)
    {
        if (excludeId is null)
        {
            return source;
        }

        return source.Where(option => option.Id != excludeId).ToList();
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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using PersonalFinance.Application.Dashboard.Requests;
using PersonalFinance.Application.Dashboard.Responses;
using PersonalFinance.Application.Dashboard.UseCases;
using PersonalFinance.Shared.Results;
using PersonalFinance.Ui.Features.Dashboard.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Features.Dashboard.ViewModels;

public sealed class DashboardPageViewModel : ObservableObject
{
    private readonly GetDashboardSummaryUseCase _summaryUseCase;
    private readonly GetDashboardCategoryBreakdownUseCase _categoryUseCase;
    private readonly GetDashboardRecentExpensesUseCase _recentUseCase;
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<DashboardPageViewModel> _logger;
    private CancellationTokenSource? _loadCts;
    private int _loadRequestId;
    private bool _isInitialized;
    private bool _isBusy;
    private int _selectedMonth;
    private int _selectedYear;
    private string _headerTitle = string.Empty;
    private string _totalSpentText = "—";
    private string _periodChangeText = "—";
    private string _dailyAverageText = "—";
    private string _expensesCountText = "0";
    private bool _hasCategoryData;
    private bool _hasRecentExpenses;
    private ObservableCollection<MonthOptionVm> _months = new();
    private ObservableCollection<int> _years = new();
    private PlotModel? _categoryChartModel;
    private ObservableCollection<RecentExpenseVm> _recentExpenses = new();

    public ObservableCollection<MonthOptionVm> Months
    {
        get => _months;
        private set => SetProperty(ref _months, value);
    }

    public ObservableCollection<int> Years
    {
        get => _years;
        private set => SetProperty(ref _years, value);
    }

    public int SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                UpdateHeaderTitle();
                if (_isInitialized)
                {
                    _ = RequestLoadAsync();
                }
            }
        }
    }

    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear, value))
            {
                UpdateHeaderTitle();
                if (_isInitialized)
                {
                    _ = RequestLoadAsync();
                }
            }
        }
    }

    public string HeaderTitle
    {
        get => _headerTitle;
        private set => SetProperty(ref _headerTitle, value);
    }

    public string TotalSpentText
    {
        get => _totalSpentText;
        private set => SetProperty(ref _totalSpentText, value);
    }

    public string PeriodChangeText
    {
        get => _periodChangeText;
        private set => SetProperty(ref _periodChangeText, value);
    }

    public string DailyAverageText
    {
        get => _dailyAverageText;
        private set => SetProperty(ref _dailyAverageText, value);
    }

    public string ExpensesCountText
    {
        get => _expensesCountText;
        private set => SetProperty(ref _expensesCountText, value);
    }

    public bool HasCategoryData
    {
        get => _hasCategoryData;
        private set => SetProperty(ref _hasCategoryData, value);
    }

    public bool HasRecentExpenses
    {
        get => _hasRecentExpenses;
        private set => SetProperty(ref _hasRecentExpenses, value);
    }

    public PlotModel? CategoryChartModel
    {
        get => _categoryChartModel;
        private set => SetProperty(ref _categoryChartModel, value);
    }

    public ObservableCollection<RecentExpenseVm> RecentExpenses
    {
        get => _recentExpenses;
        private set => SetProperty(ref _recentExpenses, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoadCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }

    public DashboardPageViewModel(
        GetDashboardSummaryUseCase summaryUseCase,
        GetDashboardCategoryBreakdownUseCase categoryUseCase,
        GetDashboardRecentExpensesUseCase recentUseCase,
        ISnackbarService snackbarService,
        ILogger<DashboardPageViewModel> logger)
    {
        _summaryUseCase = summaryUseCase;
        _categoryUseCase = categoryUseCase;
        _recentUseCase = recentUseCase;
        _snackbarService = snackbarService;
        _logger = logger;

        InitializeCommand = new AsyncRelayCommand(InitializeAsync, () => !_isInitialized);
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);

        var now = DateTime.Today;
        Months = new ObservableCollection<MonthOptionVm>(BuildMonths());
        Years = new ObservableCollection<int>(BuildYears(now.Year));
        SelectedMonth = now.Month;
        SelectedYear = now.Year;
        UpdateHeaderTitle();
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

    private Task RequestLoadAsync()
    {
        return LoadAsync();
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
            var request = BuildRequest();
            var summaryTask = _summaryUseCase.ExecuteAsync(request, ct);
            var categoryTask = _categoryUseCase.ExecuteAsync(request, ct);
            var recentTask = _recentUseCase.ExecuteAsync(request, ct);

            await Task.WhenAll(summaryTask, categoryTask, recentTask);

            if (ct.IsCancellationRequested || requestId != _loadRequestId)
            {
                return;
            }

            await ApplySummaryAsync(await summaryTask, ct);
            await ApplyCategoriesAsync(await categoryTask, ct);
            await ApplyRecentExpensesAsync(await recentTask, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (requestId == _loadRequestId)
            {
                _logger.LogError(ex, "Failed to load dashboard data.");
                _snackbarService.Show(
                    "Error",
                    GetString("Dashboard.LoadError", "Failed to load dashboard data."),
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(4));
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

    private async Task ApplySummaryAsync(Result<DashboardSummaryResponse> result, CancellationToken ct)
    {
        if (!result.IsSuccess)
        {
            ShowError(result, GetString("Dashboard.LoadSummaryError", "Failed to load summary."));
            await UpdateSummaryAsync(null, ct);
            return;
        }

        await UpdateSummaryAsync(result.Value, ct);
    }

    private async Task UpdateSummaryAsync(DashboardSummaryResponse? summary, CancellationToken ct)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            TotalSpentText = summary is null ? "—" : FormatCurrency(summary.TotalSpent);
            DailyAverageText = summary is null ? "—" : FormatCurrency(summary.DailyAverage);
            ExpensesCountText = summary is null ? "0" : summary.ExpensesCount.ToString("N0", CultureInfo.CurrentCulture);
            PeriodChangeText = summary is null ? "—" : summary.PeriodChangeText;
        }, DispatcherPriority.DataBind, ct);
    }

    private async Task ApplyCategoriesAsync(Result<IReadOnlyList<DashboardCategoryTotalResponse>> result, CancellationToken ct)
    {
        if (!result.IsSuccess)
        {
            ShowError(result, GetString("Dashboard.LoadCategoryError", "Failed to load category breakdown."));
            await UpdateCategoriesAsync(Array.Empty<CategoryTotalVm>(), ct);
            return;
        }

        var uncategorized = GetString("Expenses.CategoryNone", "Uncategorized");
        var mapped = await Task.Run(() => MapCategoryTotals(result.Value, uncategorized), ct);
        await UpdateCategoriesAsync(mapped, ct);
    }

    private async Task UpdateCategoriesAsync(IReadOnlyList<CategoryTotalVm> items, CancellationToken ct)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            CategoryChartModel = BuildCategoryChart(items);
            HasCategoryData = items.Count > 0;
        }, DispatcherPriority.DataBind, ct);
    }

    private async Task ApplyRecentExpensesAsync(Result<IReadOnlyList<DashboardRecentExpenseResponse>> result, CancellationToken ct)
    {
        if (!result.IsSuccess)
        {
            ShowError(result, GetString("Dashboard.LoadRecentError", "Failed to load recent expenses."));
            await UpdateRecentExpensesAsync(Array.Empty<RecentExpenseVm>(), ct);
            return;
        }

        var uncategorized = GetString("Expenses.CategoryNone", "Uncategorized");
        var noDescription = GetString("Expenses.DescriptionEmpty", "No description");
        var mapped = await Task.Run(() => MapRecentExpenses(result.Value, uncategorized, noDescription), ct);
        await UpdateRecentExpensesAsync(mapped, ct);
    }

    private async Task UpdateRecentExpensesAsync(IReadOnlyList<RecentExpenseVm> items, CancellationToken ct)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            RecentExpenses = new ObservableCollection<RecentExpenseVm>(items);
            HasRecentExpenses = RecentExpenses.Count > 0;
        }, DispatcherPriority.DataBind, ct);
    }

    private DashboardPeriodRequest BuildRequest()
    {
        return new DashboardPeriodRequest
        {
            Year = SelectedYear,
            Month = SelectedMonth
        };
    }

    private IReadOnlyList<MonthOptionVm> BuildMonths()
    {
        var months = new List<MonthOptionVm>(12);
        for (var i = 1; i <= 12; i++)
        {
            months.Add(new MonthOptionVm
            {
                Value = i,
                Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)
            });
        }

        return months;
    }

    private IReadOnlyList<int> BuildYears(int baseYear)
    {
        var years = new List<int>();
        for (var year = baseYear - 5; year <= baseYear + 1; year++)
        {
            years.Add(year);
        }

        return years;
    }

    private void UpdateHeaderTitle()
    {
        var format = GetString("Dashboard.TitleFormat", "Financial overview • {0:MMMM yyyy}");
        var year = SelectedYear <= 0 ? DateTime.Today.Year : SelectedYear;
        var month = SelectedMonth is < 1 or > 12 ? DateTime.Today.Month : SelectedMonth;
        var date = new DateTime(year, month, 1);
        HeaderTitle = string.Format(CultureInfo.CurrentCulture, format, date);
    }

    private static PlotModel BuildCategoryChart(IReadOnlyList<CategoryTotalVm> items)
    {
        var model = new PlotModel
        {
            IsLegendVisible = true,
            TextColor = OxyColor.FromRgb(230, 230, 230),
            Background = OxyColors.Transparent,
            PlotAreaBorderThickness = new OxyThickness(0)
        };

        model.Legends.Add(new Legend
        {
            LegendPlacement = LegendPlacement.Inside,
            LegendPosition = LegendPosition.RightTop,
            TextColor = OxyColor.FromRgb(230, 230, 230),
            LegendBackground = OxyColors.Transparent,
            LegendBorder = OxyColors.Transparent
        });

        var series = new PieSeries
        {
            StrokeThickness = 0,
            InsideLabelPosition = 0.8,
            OutsideLabelFormat = "{1} {2:0.0}%\n{0:C}",
            InsideLabelFormat = string.Empty,
            InnerDiameter = 0.6
        };

        foreach (var item in items)
        {
            series.Slices.Add(new PieSlice(item.Name, (double)item.TotalSpent)
            {
                Fill = ParseColor(item.ColorHex)
            });
        }

        model.Series.Add(series);
        return model;
    }

    private static IReadOnlyList<CategoryTotalVm> MapCategoryTotals(
        IReadOnlyList<DashboardCategoryTotalResponse>? items,
        string uncategorized)
    {
        if (items is null || items.Count == 0)
        {
            return Array.Empty<CategoryTotalVm>();
        }

        return items.Select(item => new CategoryTotalVm
        {
            Name = string.IsNullOrWhiteSpace(item.CategoryName) ? uncategorized : item.CategoryName,
            ColorHex = item.CategoryColorHex,
            TotalSpent = item.TotalSpent
        }).ToList();
    }

    private static IReadOnlyList<RecentExpenseVm> MapRecentExpenses(
        IReadOnlyList<DashboardRecentExpenseResponse>? items,
        string uncategorized,
        string noDescription)
    {
        if (items is null || items.Count == 0)
        {
            return Array.Empty<RecentExpenseVm>();
        }

        var culture = CultureInfo.CurrentCulture;

        return items.Select(item => new RecentExpenseVm
        {
            DateText = item.Date.ToString("d", culture),
            DescriptionText = string.IsNullOrWhiteSpace(item.Description) ? noDescription : item.Description,
            CategoryText = string.IsNullOrWhiteSpace(item.CategoryName) ? uncategorized : item.CategoryName,
            CategoryColorHex = item.CategoryColorHex,
            AmountText = FormatCurrency(item.Amount)
        }).ToList();
    }

    private void ShowError<T>(Result<T> result, string fallbackMessage)
    {
        var message = result.ErrorMessage ?? fallbackMessage;
        _logger.LogWarning("{Message} ({Code})", message, result.ErrorCode ?? "Unknown");
        _snackbarService.Show("Error", message, ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
    }

    private static string GetString(string key, string fallback)
    {
        return System.Windows.Application.Current.TryFindResource(key) as string ?? fallback;
    }

    private static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", CultureInfo.CurrentCulture);
    }

    private static OxyColor ParseColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return OxyColor.FromRgb(200, 200, 200);
        }

        try
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            return OxyColor.FromArgb(color.A, color.R, color.G, color.B);
        }
        catch
        {
            return OxyColor.FromRgb(200, 200, 200);
        }
    }
}

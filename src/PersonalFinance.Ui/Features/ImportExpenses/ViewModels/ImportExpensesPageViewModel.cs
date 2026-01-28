using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Application.InvoiceImport.UseCases;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Ui.Features.Categories.Models;
using PersonalFinance.Ui.Features.ImportExpenses.Models;
using PersonalFinance.Ui.Settings;
using PersonalFinance.Shared.Results;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Features.ImportExpenses.ViewModels;

public sealed class ImportExpensesPageViewModel : ObservableObject
{
    private readonly ImportDocumentUseCase _importDocumentUseCase;
    private readonly ParseDocumentUseCase _parseDocumentUseCase;
    private readonly ConfirmImportUseCase _confirmUseCase;
    private readonly IInvoiceInterpreter _invoiceInterpreter;
    private readonly ICategorySuggestionService _categorySuggestionService;
    private readonly AddVendorCategoryRuleUseCase _addVendorCategoryRuleUseCase;
    private readonly FilterCategoriesUseCase _filterCategoriesUseCase;
    private readonly IImportSetupService _setupService;
    private readonly AppSettingsStore _settingsStore;
    private readonly IContentDialogService _dialogService;
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<ImportExpensesPageViewModel> _logger;

    private CancellationTokenSource? _loadCts;
    private int _loadRequestId;
    private bool _isBusy;
    private bool _isInitialized;
    private string _selectedFileName = string.Empty;
    private Guid _documentId;
    private string _vendorName = string.Empty;
    private IReadOnlyList<CategoryLookupItemVm> _categoryOptions = Array.Empty<CategoryLookupItemVm>();
    private ObservableCollection<ExpenseDraftReviewItemVm> _items = new();
    private ObservableCollection<ImportSetupItemVm> _setupItems = new();
    private bool _isSetupRequired;
    private IReadOnlyList<OcrLanguageOption> _ocrLanguageOptions = Array.Empty<OcrLanguageOption>();
    private OcrLanguageOption? _selectedOcrLanguage;
    private bool _canDownloadSetup;
    private string _totalAmountText = 0m.ToString("C", CultureInfo.CurrentCulture);
    private bool _isDragOver;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                PickFileCommand.NotifyCanExecuteChanged();
                ConfirmCommand.NotifyCanExecuteChanged();
                ClearCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasItems => Items.Count > 0;

    public string SelectedFileName
    {
        get => _selectedFileName;
        private set => SetProperty(ref _selectedFileName, value);
    }

    public IReadOnlyList<CategoryLookupItemVm> CategoryOptions
    {
        get => _categoryOptions;
        private set => SetProperty(ref _categoryOptions, value);
    }

    public ObservableCollection<ExpenseDraftReviewItemVm> Items
    {
        get => _items;
        private set
        {
            if (SetProperty(ref _items, value))
            {
                OnPropertyChanged(nameof(HasItems));
                UpdateTotals();
            }
        }
    }

    public string TotalAmountText
    {
        get => _totalAmountText;
        private set => SetProperty(ref _totalAmountText, value);
    }

    public ObservableCollection<ImportSetupItemVm> SetupItems
    {
        get => _setupItems;
        private set => SetProperty(ref _setupItems, value);
    }

    public bool IsSetupRequired
    {
        get => _isSetupRequired;
        private set => SetProperty(ref _isSetupRequired, value);
    }

    public IReadOnlyList<OcrLanguageOption> OcrLanguageOptions
    {
        get => _ocrLanguageOptions;
        private set => SetProperty(ref _ocrLanguageOptions, value);
    }

    public OcrLanguageOption? SelectedOcrLanguage
    {
        get => _selectedOcrLanguage;
        set
        {
            if (SetProperty(ref _selectedOcrLanguage, value) && value is not null)
            {
                var settings = _settingsStore.LoadOrDefault();
                settings.OcrLanguageCode = value.Code;
                _settingsStore.Save(settings);
                _ = LoadSetupStatusAsync();
            }
        }
    }

    public bool CanDownloadSetup
    {
        get => _canDownloadSetup;
        private set => SetProperty(ref _canDownloadSetup, value);
    }

    public bool IsDragOver
    {
        get => _isDragOver;
        set => SetProperty(ref _isDragOver, value);
    }

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand PickFileCommand { get; }
    public IAsyncRelayCommand ConfirmCommand { get; }
    public IAsyncRelayCommand DownloadSetupCommand { get; }
    public IRelayCommand ClearCommand { get; }

    public ImportExpensesPageViewModel(
        ImportDocumentUseCase importDocumentUseCase,
        ParseDocumentUseCase parseDocumentUseCase,
        ConfirmImportUseCase confirmUseCase,
        IInvoiceInterpreter invoiceInterpreter,
        ICategorySuggestionService categorySuggestionService,
        AddVendorCategoryRuleUseCase addVendorCategoryRuleUseCase,
        FilterCategoriesUseCase filterCategoriesUseCase,
        IImportSetupService setupService,
        AppSettingsStore settingsStore,
        IContentDialogService dialogService,
        ISnackbarService snackbarService,
        ILogger<ImportExpensesPageViewModel> logger)
    {
        _importDocumentUseCase = importDocumentUseCase;
        _parseDocumentUseCase = parseDocumentUseCase;
        _confirmUseCase = confirmUseCase;
        _invoiceInterpreter = invoiceInterpreter;
        _categorySuggestionService = categorySuggestionService;
        _addVendorCategoryRuleUseCase = addVendorCategoryRuleUseCase;
        _filterCategoriesUseCase = filterCategoriesUseCase;
        _setupService = setupService;
        _settingsStore = settingsStore;
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        _logger = logger;

        InitializeCommand = new AsyncRelayCommand(InitializeAsync, () => !_isInitialized);
        PickFileCommand = new AsyncRelayCommand(PickFileAsync, () => !IsBusy);
        ConfirmCommand = new AsyncRelayCommand(ConfirmAsync, () => !IsBusy && Items.Count > 0);
        DownloadSetupCommand = new AsyncRelayCommand(DownloadSetupAsync, () => !IsBusy && CanDownloadSetup);
        ClearCommand = new RelayCommand(Clear);
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        InitializeCommand.NotifyCanExecuteChanged();
        InitializeSetupItems();
        LoadOcrLanguageOptions();
        await LoadSetupStatusAsync();
        await LoadCategoriesAsync();
    }

    private async Task PickFileAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (IsSetupRequired)
        {
            _snackbarService.Show("Setup", "Complete the import setup before selecting a file.", ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Documents (*.pdf;*.png;*.jpg;*.jpeg)|*.pdf;*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await ProcessFileAsync(dialog.FileName, dialog.SafeFileName);
    }

    public async Task HandleDropAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        if (IsSetupRequired)
        {
            _snackbarService.Show("Setup", "Complete the import setup before selecting a file.", ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
            return;
        }

        await ProcessFileAsync(filePath, Path.GetFileName(filePath));
    }

    private async Task ProcessFileAsync(string filePath, string originalFileName)
    {
        if (IsBusy)
        {
            return;
        }

        if (IsSetupRequired)
        {
            _snackbarService.Show("Setup", "Complete the import setup before selecting a file.", ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
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
            SelectedFileName = originalFileName;
            Items.Clear();

            var importResult = await _importDocumentUseCase.ExecuteAsync(new ImportDocumentRequest
            {
                SourceFilePath = filePath,
                OriginalFileName = originalFileName
            }, ct);

            if (!EnsureSuccess(importResult, "Import failed."))
            {
                return;
            }

            _documentId = importResult.Value!.DocumentId;

            var parseResult = await _parseDocumentUseCase.ExecuteAsync(new ParseDocumentRequest
            {
                DocumentId = _documentId
            }, ct);

            if (!EnsureSuccess(parseResult, "Parse failed."))
            {
                return;
            }

            var parsed = parseResult.Value!;
            var interpretation = await _invoiceInterpreter.InterpretAsync(new InterpretInvoiceRequest
            {
                RawText = parsed.RawText
            }, ct);

            if (!interpretation.IsSuccess)
            {
                ShowError(interpretation, "Failed to interpret document.");
                return;
            }

            _vendorName = interpretation.IsSuccess
                ? interpretation.Value!.Data.VendorName
                : string.Empty;

            var interpretationLineItems = interpretation.IsSuccess
                ? interpretation.Value!.Data.LineItems
                : null;

            var lineItems = interpretationLineItems?.Select(x => x.Description).ToList() ?? new List<string>();

            var suggestionsResult = await BuildSuggestionsAsync(parsed, interpretation.Value!.Data, lineItems, ct);
            if (!EnsureSuccess(suggestionsResult, "Failed to suggest categories."))
            {
                return;
            }

            var suggestions = suggestionsResult.Value!;
            if (ct.IsCancellationRequested || requestId != _loadRequestId)
            {
                return;
            }

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Items = new ObservableCollection<ExpenseDraftReviewItemVm>(suggestions);
                SubscribeItems(Items);
                OnPropertyChanged(nameof(HasItems));
                ConfirmCommand.NotifyCanExecuteChanged();
                UpdateTotals();
            }, DispatcherPriority.DataBind, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (requestId == _loadRequestId)
            {
                _logger.LogError(ex, "Failed to import expenses.");
                _snackbarService.Show("Error", "Failed to import expenses.", ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
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

    private async Task LoadSetupStatusAsync()
    {
        try
        {
            var status = await _setupService.GetStatusAsync(CancellationToken.None);
            var items = status.Select(item => new ImportSetupItemVm
            {
                Key = item.Key,
                Title = GetSetupTitle(item),
                LanguageCode = item.LanguageCode,
                ModelName = item.ModelName,
                StatusCode = item.Detail,
                IsRequired = item.IsRequired,
                IsInstalled = item.IsInstalled,
                StatusText = GetSetupDetail(item),
                Progress = item.IsInstalled ? 100 : 0
            }).ToList();

            SetupItems = new ObservableCollection<ImportSetupItemVm>(items);
            IsSetupRequired = items.Any(x => x.IsRequired && !x.IsInstalled);
            CanDownloadSetup = items.Any(IsDownloadable);
            DownloadSetupCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load import setup status.");
        }
    }

    private void LoadOcrLanguageOptions()
    {
        OcrLanguageOptions = new List<OcrLanguageOption>
        {
            new("eng", GetResource("ImportSetup.Language.English", "English")),
            new("por", GetResource("ImportSetup.Language.Portuguese", "Portuguese"))
        };

        var settings = _settingsStore.LoadOrDefault();
        var code = string.IsNullOrWhiteSpace(settings.OcrLanguageCode)
            ? (settings.CultureName.Equals("pt-BR", StringComparison.OrdinalIgnoreCase) ? "por" : "eng")
            : settings.OcrLanguageCode.Trim().ToLowerInvariant();

        SelectedOcrLanguage = OcrLanguageOptions.FirstOrDefault(x => x.Code == code) ?? OcrLanguageOptions.First();
    }

    private string GetSetupTitle(ImportSetupItemStatus item)
    {
        return item.Key switch
        {
            "tesseract" when !string.IsNullOrWhiteSpace(item.LanguageCode)
                => string.Format(GetResource("ImportSetup.Title.OcrLanguageFormat", "Tesseract OCR ({0})"), GetOcrLanguageDisplay(item.LanguageCode)),
            "tesseract" => GetResource("ImportSetup.Title.Tesseract", "Tesseract OCR"),
            "llm" when !string.IsNullOrWhiteSpace(item.ModelName)
                => string.Format(GetResource("ImportSetup.Title.LlmModelFormat", "LLM model ({0})"), item.ModelName),
            "llm" => GetResource("ImportSetup.Title.Llm", "LLM model"),
            _ => item.Title
        };
    }

    private string GetSetupDetail(ImportSetupItemStatus item)
    {
        return item.Detail switch
        {
            "Installed" => GetResource("ImportSetup.Status.Installed", "Installed"),
            "DownloadRequired" => GetResource("ImportSetup.Status.DownloadRequired", "Download required"),
            "Checking" => GetResource("ImportSetup.Status.Checking", "Checking"),
            _ => item.Detail ?? string.Empty
        };
    }

    private string GetProgressText(string code)
    {
        return code switch
        {
            "Starting" => GetResource("ImportSetup.Progress.Starting", "Starting"),
            "Downloading" => GetResource("ImportSetup.Progress.Downloading", "Downloading"),
            "Completed" => GetResource("ImportSetup.Progress.Completed", "Completed"),
            "NotRequired" => GetResource("ImportSetup.Progress.NotRequired", "Not required"),
            "ModelNotConfigured" => GetResource("ImportSetup.Status.ModelNotConfigured", "Model not configured"),
            _ => code
        };
    }

    private static bool IsDownloadable(ImportSetupItemVm item)
    {
        if (item.IsInstalled)
        {
            return false;
        }

        return item.Key switch
        {
            "tesseract" => true,
            "llm" => string.Equals(item.StatusCode, "DownloadRequired", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private void InitializeSetupItems()
    {
        SetupItems = new ObservableCollection<ImportSetupItemVm>
        {
            new()
            {
                Key = "tesseract",
                Title = GetResource("ImportSetup.Title.Tesseract", "Tesseract OCR"),
                IsRequired = true,
                IsInstalled = false,
                StatusText = GetResource("ImportSetup.Status.Checking", "Checking"),
                StatusCode = "Checking",
                Progress = 0
            },
            new()
            {
                Key = "llm",
                Title = GetResource("ImportSetup.Title.Llm", "LLM model"),
                IsRequired = true,
                IsInstalled = false,
                StatusText = GetResource("ImportSetup.Status.Checking", "Checking"),
                StatusCode = "Checking",
                Progress = 0
            }
        };

        IsSetupRequired = true;
        CanDownloadSetup = false;
    }



    private static string GetResource(string key, string fallback)
    {
        if (System.Windows.Application.Current?.TryFindResource(key) is string value && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    private string GetOcrLanguageDisplay(string languageCode)
    {
        var normalized = languageCode.Trim().ToLowerInvariant();
        return normalized switch
        {
            "por" => GetResource("ImportSetup.Language.Portuguese", "Portuguese"),
            "eng" => GetResource("ImportSetup.Language.English", "English"),
            _ => normalized
        };
    }

    private async Task DownloadSetupAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var targets = SetupItems.Where(IsDownloadable).ToList();
        if (targets.Count == 0)
        {
            _snackbarService.Show("Setup", "No downloadable components. Configure the LLM first.", ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
            return;
        }

        IsBusy = true;
        try
        {
            var progress = new Progress<ImportSetupProgress>(update =>
            {
                var item = SetupItems.FirstOrDefault(x => x.Key == update.Key);
                if (item is null)
                {
                    return;
                }

                item.Progress = update.Percent;
                if (!string.IsNullOrWhiteSpace(update.Message))
                {
                    item.StatusText = GetProgressText(update.Message);
                }

                if (update.Percent >= 100)
                {
                    item.IsInstalled = true;
                }
            });

            foreach (var item in targets)
            {
                await _setupService.DownloadAsync(item.Key, progress, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download import setup.");
            _snackbarService.Show("Error", "Failed to download import setup.", ControlAppearance.Danger, null, TimeSpan.FromSeconds(4));
        }
        finally
        {
            await LoadSetupStatusAsync();
            IsBusy = false;
        }
    }

    private async Task<Result<IReadOnlyList<ExpenseDraftReviewItemVm>>> BuildSuggestionsAsync(
        ParseDocumentResponse parsed,
        InvoiceInterpretationResponse interpretation,
        IReadOnlyList<string> lineItems,
        CancellationToken ct)
    {
        var suggestion = await _categorySuggestionService.SuggestAsync(new CategorySuggestionRequest
        {
            VendorName = _vendorName,
            RawText = parsed.RawText,
            LineItems = lineItems
        }, ct);

        if (!suggestion.IsSuccess)
        {
            _logger.LogWarning("Category suggestion failed: {Message}", suggestion.ErrorMessage ?? "Unknown");
            _snackbarService.Show("Atenção", "Não foi possível sugerir categorias. Os itens foram carregados sem sugestão.", ControlAppearance.Caution, null, TimeSpan.FromSeconds(4));
        }

        var suggestedId = suggestion.IsSuccess ? suggestion.Value!.SuggestedCategoryId : null;
        var fallbackCategoryId = suggestedId ?? _categoryOptions.FirstOrDefault()?.Id;
        var confidence = suggestion.IsSuccess ? suggestion.Value!.Confidence : 0d;
        var items = BuildDraftItems(parsed, interpretation).Select(item => new ExpenseDraftReviewItemVm
        {
            DateValue = item.Date,
            AmountValue = item.Amount,
            Description = item.Description ?? string.Empty,
            ConfidenceValue = confidence,
            SuggestedCategoryId = suggestedId,
            SelectedCategoryId = suggestedId ?? fallbackCategoryId
        }).ToList();

        return Result<IReadOnlyList<ExpenseDraftReviewItemVm>>.Success(items);
    }

    private static IReadOnlyList<ExpenseDraftItemResponse> BuildDraftItems(
        ParseDocumentResponse parsed,
        InvoiceInterpretationResponse interpretation)
    {
        if (interpretation.LineItems is { Count: > 0 })
        {
            var date = interpretation.InvoiceDate == default
                ? DateTime.Today
                : interpretation.InvoiceDate;

            var items = interpretation.LineItems
                .Select(item => new
                {
                    Description = item.Description?.Trim(),
                    Amount = item.TotalPrice ?? item.UnitPrice
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Description) && item.Amount is > 0m)
                .Select(item => new ExpenseDraftItemResponse
                {
                    Date = date,
                    Amount = item.Amount!.Value,
                    Description = item.Description,
                    CategoryId = null,
                    CategoryName = null,
                    Confidence = 0.6d
                })
                .ToList();

            if (items.Count > 0)
            {
                return items;
            }
        }

        return parsed.Items;
    }

    private async Task LoadCategoriesAsync()
    {
        var request = new FilterCategoriesRequest
        {
            IncludeAll = true,
            SortBy = "Name",
            SortDescending = false,
            Page = null,
            ParentId = null
        };

        var result = await _filterCategoriesUseCase.ExecuteAsync(request, CancellationToken.None);
        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to load categories.");
            CategoryOptions = Array.Empty<CategoryLookupItemVm>();
            return;
        }

        CategoryOptions = result.Value!.Items.Select(item => new CategoryLookupItemVm
        {
            Id = item.Id,
            Name = item.Name,
            Depth = 0,
            DisplayName = item.Name
        }).ToList();
    }

    private void SubscribeItems(IEnumerable<ExpenseDraftReviewItemVm> items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private async void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not ExpenseDraftReviewItemVm item)
        {
            return;
        }

        if (e.PropertyName == nameof(ExpenseDraftReviewItemVm.AmountValue))
        {
            UpdateTotals();
            return;
        }

        if (e.PropertyName != nameof(ExpenseDraftReviewItemVm.SelectedCategoryId))
        {
            return;
        }

        if (item.SelectedCategoryId is null || item.SelectedCategoryId == item.SuggestedCategoryId)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_vendorName))
        {
            return;
        }

        var result = await _addVendorCategoryRuleUseCase.ExecuteAsync(new AddVendorCategoryRuleRequest
        {
            Keyword = _vendorName,
            CategoryId = item.SelectedCategoryId.Value,
            Confidence = 0.7d
        }, CancellationToken.None);

        if (!result.IsSuccess)
        {
            ShowError(result, "Failed to save category rule.");
        }
    }

    private void UpdateTotals()
    {
        var total = Items.Sum(item => item.AmountValue);
        TotalAmountText = total.ToString("C", CultureInfo.CurrentCulture);
    }

    private async Task ConfirmAsync()
    {
        if (Items.Count == 0 || IsBusy)
        {
            return;
        }

        var dialog = new ContentDialog(_dialogService.GetDialogHostEx())
        {
            Title = "Confirm import",
            Content = "Save imported expenses?",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel"
        };

        var result = await _dialogService.ShowAsync(dialog, CancellationToken.None);
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var confirmRequest = new ConfirmImportRequest
            {
                DocumentId = _documentId,
                Items = Items.Select(item => new ExpenseDraftItemRequest
                {
                    Date = item.DateValue,
                    Amount = item.AmountValue,
                    Description = item.Description,
                    CategoryId = item.SelectedCategoryId,
                    CategoryName = null,
                    Confidence = 0.7d
                }).ToList()
            };

            var confirmResult = await _confirmUseCase.ExecuteAsync(confirmRequest, CancellationToken.None);
            if (!EnsureSuccess(confirmResult, "Failed to save expenses."))
            {
                return;
            }

            _snackbarService.Show("Success", "Import completed.", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
            Clear();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Clear()
    {
        Items.Clear();
        SelectedFileName = string.Empty;
        _documentId = Guid.Empty;
        _vendorName = string.Empty;
        OnPropertyChanged(nameof(HasItems));
        ConfirmCommand.NotifyCanExecuteChanged();
        UpdateTotals();
    }

    private bool EnsureSuccess<T>(Result<T> result, string fallbackMessage)
    {
        if (result.IsSuccess)
        {
            return true;
        }

        ShowError(result, fallbackMessage);
        return false;
    }

    private bool EnsureSuccess(Result result, string fallbackMessage)
    {
        if (result.IsSuccess)
        {
            return true;
        }

        ShowError(result, fallbackMessage);
        return false;
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
}

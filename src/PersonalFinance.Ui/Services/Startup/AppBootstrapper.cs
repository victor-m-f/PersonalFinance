using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Dashboard.Abstractions;
using PersonalFinance.Application.Dashboard.UseCases;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Application.Categories.Validators;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.UseCases;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Settings;
using PersonalFinance.Application.InvoiceImport.UseCases;
using PersonalFinance.Infrastructure.Documents;
using PersonalFinance.Infrastructure.InvoiceImport;
using PersonalFinance.Infrastructure.Data;
using PersonalFinance.Infrastructure.Data.Repositories;
using PersonalFinance.Ui.Features.Categories.Services;
using PersonalFinance.Ui.Features.Categories.ViewModels;
using PersonalFinance.Ui.Features.Categories.Views;
using PersonalFinance.Ui.Features.Categories.Views.Dialogs;
using PersonalFinance.Ui.Features.Dashboard.ViewModels;
using PersonalFinance.Ui.Features.Dashboard.Views;
using PersonalFinance.Ui.Features.Expenses.ViewModels;
using PersonalFinance.Ui.Features.Expenses.Views;
using PersonalFinance.Ui.Features.Expenses.Views.Dialogs;
using PersonalFinance.Ui.Features.ImportExpenses.ViewModels;
using PersonalFinance.Ui.Features.ImportExpenses.Views;
using PersonalFinance.Ui.Features.ImportExpenses.Services;
using PersonalFinance.Ui.Features.Settings.Components.ViewModels;
using PersonalFinance.Ui.Features.Settings.Views;
using PersonalFinance.Ui.Helpers;
using PersonalFinance.Ui.Services.Localization;
using PersonalFinance.Ui.Settings;
using PersonalFinance.Ui.Shared.Shell;
using PersonalFinance.Ui.Shared.Shell.Services;
using PersonalFinance.Ui.Shared.Shell.ViewModels;
using Serilog;
using Serilog.Events;
using System.Net.Http;
using System.IO;
using Wpf.Ui;

namespace PersonalFinance.Ui.Services.Startup;

public static class AppBootstrapper
{
    public static IHost BuildHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.Configure<InvoiceInterpreterOptions>(
                    context.Configuration.GetSection("InvoiceInterpreter"));
                services.Configure<CategorySuggestionOptions>(
                    context.Configuration.GetSection("CategorySuggestion"));
                services.Configure<LlmProviderOptions>(
                    context.Configuration.GetSection("InvoiceInterpreter:Llm"));
                services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
                services.AddSingleton<AppSettingsStore>();
                services.AddSingleton<IOcrLanguageProvider, OcrLanguageProvider>();
                services.AddSingleton<ILlmSettingsProvider, LlmSettingsProvider>();
                services.AddSingleton<ILocalizationService, LocalizationService>();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();
                services.AddSingleton<Wpf.Ui.Abstractions.INavigationViewPageProvider, NavigationViewPageProvider>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<ShellWindow>();
                services.AddTransient<ThemeSettingsViewModel>();
                services.AddTransient<LanguageSettingsViewModel>();
                services.AddTransient<LoggingSettingsViewModel>();
                services.AddTransient<ImportSettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ExpensesPageViewModel>();
                services.AddTransient<ExpensesPage>();
                services.AddTransient<ImportExpensesPageViewModel>();
                services.AddTransient<ImportExpensesPage>();
                services.AddTransient<ExpenseEditorDialogViewModel>();
                services.AddTransient<ExpenseEditorDialog>();
                services.AddTransient<DashboardPageViewModel>();
                services.AddTransient<DashboardPage>();
                services.AddTransient<CategoriesPageViewModel>();
                services.AddTransient<CategoriesPage>();
                services.AddTransient<CategoryEditorDialogViewModel>();
                services.AddTransient<CategoryEditorDialog>();
                services.AddSingleton<IUiDialogService, UiDialogService>();
                services.AddDbContext<PersonalFinanceDbContext>(options =>
                {
                    Directory.CreateDirectory(FileSystemPaths.LocalAppDataFolder);
                    var dbPath = Path.Combine(FileSystemPaths.LocalAppDataFolder, "personalfinance.db");
                    options.UseSqlite($"Data Source={dbPath}");
                });
                services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IExpenseRepository, ExpenseRepository>();
                services.AddScoped<IExpenseReadRepository, ExpenseReadRepository>();
                services.AddScoped<IDashboardReadRepository, DashboardReadRepository>();
                services.AddScoped<IImportedDocumentRepository, ImportedDocumentRepository>();
                services.AddScoped<IImportedDocumentStorage, LocalImportedDocumentStorage>();
                services.AddScoped<IDocumentTextExtractor, DocumentTextExtractor>();
                services.AddScoped<ICategorySuggestionRepository, CategorySuggestionRepository>();
                services.AddScoped<IVendorCategoryRuleRepository, VendorCategoryRuleRepository>();
                services.AddSingleton<CategorySuggestionCache>();
                services.AddSingleton<ICategorySuggestionCache>(sp => sp.GetRequiredService<CategorySuggestionCache>());
                services.AddSingleton<ICategorySuggestionCacheInvalidator>(sp => sp.GetRequiredService<CategorySuggestionCache>());
                services.AddScoped<ICategorySuggestionService, CategorySuggestionService>();
                services.AddScoped<LlmJsonInterpreter>();
                services.AddScoped<IImportSetupService, ImportSetupService>();
                services.AddScoped<IInvoiceInterpreter, InvoiceInterpreter>();
                services.AddSingleton<LocalLlmModelStore>();
                services.AddSingleton<LocalLlmRuntime>();
                services.AddSingleton(new HttpClient());
                services.AddScoped<FilterExpensesUseCase>();
                services.AddScoped<CreateExpenseUseCase>();
                services.AddScoped<UpdateExpenseUseCase>();
                services.AddScoped<DeleteExpenseUseCase>();
                services.AddScoped<AssignExpenseCategoryUseCase>();
                services.AddScoped<GetDashboardSummaryUseCase>();
                services.AddScoped<GetDashboardCategoryBreakdownUseCase>();
                services.AddScoped<GetDashboardRecentExpensesUseCase>();
                services.AddScoped<ImportDocumentUseCase>();
                services.AddScoped<ParseDocumentUseCase>();
                services.AddScoped<ReviewDraftUseCase>();
                services.AddScoped<ConfirmImportUseCase>();
                services.AddScoped<AddVendorCategoryRuleUseCase>();
                services.AddScoped<FilterCategoriesUseCase>();
                services.AddScoped<CreateCategoryUseCase>();
                services.AddScoped<UpdateCategoryUseCase>();
                services.AddScoped<DeleteCategoryUseCase>();
            })
            .UseSerilog((context, services, loggerConfiguration) =>
            {
                var settingsStore = new AppSettingsStore(NullLogger<AppSettingsStore>.Instance);
                var settings = settingsStore.LoadOrDefault();
                var level = settings.VerboseLogging ? LogEventLevel.Debug : LogEventLevel.Information;
                Directory.CreateDirectory(FileSystemPaths.LogsFolder);
                loggerConfiguration
                    .MinimumLevel.Is(level)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        Path.Combine(FileSystemPaths.LogsFolder, "app-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14,
                        shared: true);
            })
            .Build();
    }

    public static void InitializeLocalization(
        IServiceProvider services,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        var localizationService = services.GetRequiredService<ILocalizationService>();
        logger.LogInformation("Localization initialization starting");
        localizationService.InitializeFromSettings();
        logger.LogInformation("Localization initialization completed");
    }

    public static AppSettings LoadSettings(
        IServiceProvider services,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        var settingsStore = services.GetRequiredService<AppSettingsStore>();
        var settings = settingsStore.LoadOrDefault();
        logger.LogInformation("Settings loaded");
        return settings;
    }

    public static ShellWindow CreateMainWindow(
        IServiceProvider services,
        Microsoft.Extensions.Logging.ILogger logger, Action onLoaded)
    {
        logger.LogInformation("Creating main window");
        var window = services.GetRequiredService<ShellWindow>();
        window.Loaded += (_, _) =>
        {
            onLoaded();
            logger.LogInformation("Main window loaded");
        };
        return window;
    }
}

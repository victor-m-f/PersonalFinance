using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PersonalFinance.Application.Categories.Validators;
using PersonalFinance.Ui.Features.Categories.ViewModels;
using PersonalFinance.Ui.Features.Categories.Views;
using PersonalFinance.Ui.Features.Expenses.ViewModels;
using PersonalFinance.Ui.Features.Expenses.Views;
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
using System.IO;
using Wpf.Ui;

namespace PersonalFinance.Ui.Services.Startup;

public static class AppBootstrapper
{
    public static IHost BuildHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
                services.AddSingleton<AppSettingsStore>();
                services.AddSingleton<ILocalizationService, LocalizationService>();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<Wpf.Ui.Abstractions.INavigationViewPageProvider, NavigationViewPageProvider>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<ShellWindow>();
                services.AddTransient<ThemeSettingsViewModel>();
                services.AddTransient<LanguageSettingsViewModel>();
                services.AddTransient<LoggingSettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ExpensesPageViewModel>();
                services.AddTransient<ExpensesPage>();
                services.AddTransient<CategoriesPageViewModel>();
                services.AddTransient<CategoriesPage>();
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

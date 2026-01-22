using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalFinance.Ui.Features.Categories.ViewModels;
using PersonalFinance.Ui.Features.Categories.Views;
using PersonalFinance.Ui.Features.Expenses.ViewModels;
using PersonalFinance.Ui.Features.Expenses.Views;
using PersonalFinance.Ui.Features.Settings.ViewModels;
using PersonalFinance.Ui.Features.Settings.Views;
using PersonalFinance.Ui.Services.Localization;
using PersonalFinance.Ui.Settings;
using PersonalFinance.Ui.Shared.Shell;
using PersonalFinance.Ui.Shared.Shell.ViewModels;
using System.Windows;

namespace PersonalFinance.Ui;

public partial class App : System.Windows.Application
{
	private IHost? _host;

	public static IServiceProvider Services { get; private set; } = default!;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		_host = Host.CreateDefaultBuilder()
			.ConfigureServices(services =>
			{
				services.AddSingleton<AppSettingsStore>();
				services.AddSingleton<ILocalizationService, LocalizationService>();
				services.AddSingleton<ShellViewModel>();
				services.AddSingleton<ShellWindow>();
				services.AddTransient<SettingsPageViewModel>();
				services.AddTransient<SettingsPage>();
				services.AddTransient<ExpensesPageViewModel>();
				services.AddTransient<ExpensesPage>();
				services.AddTransient<CategoriesPageViewModel>();
				services.AddTransient<CategoriesPage>();
			})
			.Build();

		Services = _host.Services;

		var localizationService = Services.GetRequiredService<ILocalizationService>();
		localizationService.InitializeFromSettings();

		var settingsStore = Services.GetRequiredService<AppSettingsStore>();
		var settings = settingsStore.LoadOrDefault();

		var window = Services.GetRequiredService<ShellWindow>();
		MainWindow = window;
		ThemeApplier.Apply(settings);
		window.Show();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_host?.Dispose();
		base.OnExit(e);
	}
}

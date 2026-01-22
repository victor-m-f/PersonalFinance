using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Services.Startup;
using PersonalFinance.Ui.Settings;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui;

public partial class App : System.Windows.Application
{
	private IHost? _host;
	private bool _mainWindowShown;

	public static IServiceProvider Services { get; private set; } = default!;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		try
		{
			_host = AppBootstrapper.BuildHost();
			Services = _host.Services;
			RegisterGlobalExceptionHandlers();
			var logger = Services.GetRequiredService<ILogger<App>>();
			logger.LogInformation("Application starting");

			AppBootstrapper.InitializeLocalization(Services, logger);
			var settings = AppBootstrapper.LoadSettings(Services, logger);
			try
			{
				var window = AppBootstrapper.CreateMainWindow(Services, logger, () => _mainWindowShown = true);
				MainWindow = window;
				window.Show();
				logger.LogInformation("Main window shown");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to create or show main window");
				Log.CloseAndFlush();
				Shutdown();
				return;
			}

			try
			{
				ThemeApplier.Apply(settings);
				logger.LogInformation("Theme applied at startup: {Theme}", settings.Theme);
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Failed to apply theme at startup");
			}
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Startup failed");
			Shutdown();
		}
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_host?.Dispose();
		Log.CloseAndFlush();
		base.OnExit(e);
	}

	private void RegisterGlobalExceptionHandlers()
	{
		DispatcherUnhandledException += OnDispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
	}

	private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		HandleException(e.Exception, "DispatcherUnhandledException");
		e.Handled = true;
	}

	private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		var exception = e.ExceptionObject as Exception;
		HandleException(exception, "AppDomainUnhandledException");
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		HandleException(e.Exception, "UnobservedTaskException");
		e.SetObserved();
	}

	private void HandleException(Exception? exception, string source)
	{
		try
		{
			var logger = Services.GetRequiredService<ILogger<App>>();
			if (exception is null)
			{
				logger.LogError("Unhandled exception without details from {Source}", source);
			}
			else
			{
				logger.LogError(exception, "Unhandled exception from {Source}", source);
			}
		}
		catch
		{
		}

		if (!_mainWindowShown)
		{
			return;
		}

		try
		{
			Dispatcher.Invoke(() =>
			{
				var snackbarService = Services.GetService<ISnackbarService>();
				snackbarService?.Show(
					"Error",
					"An unexpected error occurred.",
					ControlAppearance.Danger,
					null,
					TimeSpan.FromSeconds(4));
			});
		}
		catch
		{
		}
	}
}

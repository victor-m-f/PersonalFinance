using PersonalFinance.Ui.Views.Shell;
using PersonalFinance.Ui.Settings;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui;

public partial class App : System.Windows.Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		var settingsStore = new AppSettingsStore();
		var settings = settingsStore.LoadOrDefault();

		var window = new ShellWindow();
		MainWindow = window;
		ThemeApplier.Apply(settings);
		window.Show();
	}
}

using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using PersonalFinance.Ui.Views.Shell;

namespace PersonalFinance.Ui;

public partial class App : System.Windows.Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		var window = new ShellWindow();
		MainWindow = window;
		ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.None);
		window.Show();
	}
}

using System.Windows;
using PersonalFinance.Ui.Views;

namespace PersonalFinance.Ui;

public partial class App : System.Windows.Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		var window = new ShellWindow();
		window.Show();
	}
}

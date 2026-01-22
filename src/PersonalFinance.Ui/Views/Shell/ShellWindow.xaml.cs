using PersonalFinance.Ui.Views.Pages.Expenses;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Views.Shell;

public partial class ShellWindow : FluentWindow
{
    public ShellWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        ApplicationThemeManager.Apply(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        NavigationView.NavigateWithHierarchy(typeof(ExpensesPage));
    }

    private void OnNavigationSelectionChanged(NavigationView sender, RoutedEventArgs e)
    {
        if (sender.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        if (item.TargetPageType is null)
        {
            return;
        }

        sender.NavigateWithHierarchy(item.TargetPageType);
    }

    private void OnBackRequested(object sender, RoutedEventArgs e)
    {
        if (NavigationView.IsBackEnabled)
        {
            NavigationView.GoBack();
        }
    }
}

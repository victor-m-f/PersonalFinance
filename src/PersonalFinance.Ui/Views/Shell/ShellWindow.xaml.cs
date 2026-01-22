using PersonalFinance.Ui.Views.Pages.Expenses;
using System.Windows;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Views.Shell;

public partial class ShellWindow : FluentWindow
{
    public ShellWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        var pageType = typeof(ExpensesPage);
        NavigationView.NavigateWithHierarchy(pageType);
    }

    private void OnNavigationSelectionChanged(NavigationView sender, RoutedEventArgs e)
    {
        var item = sender.SelectedItem as NavigationViewItem;
        if (item is null)
        {
            return;
        }

        var targetPageType = item.TargetPageType;
        if (targetPageType is null)
        {
            return;
        }

        sender.NavigateWithHierarchy(targetPageType);
    }

    private void OnBackRequested(object sender, RoutedEventArgs e)
    {
        if (NavigationView.IsBackEnabled)
        {
            NavigationView.GoBack();
        }
    }
}

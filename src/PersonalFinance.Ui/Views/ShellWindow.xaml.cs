using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using PersonalFinance.Ui.Views.Pages;

namespace PersonalFinance.Ui.Views;

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
        NavigationView.Navigate(typeof(ExpensesPage));
    }

    private void OnNavigationItemInvoked(NavigationView sender, RoutedEventArgs e)
    {
        if (sender.SelectedItem is NavigationViewItem item)
        {
            NavigateTo(item);
        }
    }

    private void OnNavigationSelectionChanged(NavigationView sender, RoutedEventArgs e)
    {
        if (sender.SelectedItem is NavigationViewItem item)
        {
            NavigateTo(item);
        }
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        if (NavigationView.CanGoBack)
        {
            NavigationView.GoBack();
        }
    }

    private void NavigateTo(NavigationViewItem item)
    {
        if (item.TargetPageType is null)
        {
            return;
        }

        NavigationView.Navigate(item.TargetPageType);
    }
}

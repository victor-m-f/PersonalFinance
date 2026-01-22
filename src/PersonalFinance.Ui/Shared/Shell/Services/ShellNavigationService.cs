using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Shared.Shell.Services;

public sealed class ShellNavigationService
{
    private INavigationView? _navigationView;

    public static ShellNavigationService Instance { get; } = new();

    public bool IsBackEnabled => _navigationView?.IsBackEnabled ?? false;

    public void SetNavigationView(INavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    public bool Navigate(Type pageType)
    {
        if (_navigationView is null)
        {
            return false;
        }

        if (_navigationView.SelectedItem is INavigationViewItem selectedItem)
        {
            if (selectedItem.TargetPageType == pageType)
            {
                return true;
            }
        }

        return _navigationView.NavigateWithHierarchy(pageType);
    }

    public bool GoBack()
    {
        return _navigationView?.GoBack() ?? false;
    }
}

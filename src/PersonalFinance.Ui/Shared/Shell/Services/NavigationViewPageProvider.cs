using Wpf.Ui.Abstractions;

namespace PersonalFinance.Ui.Shared.Shell.Services;

public sealed class NavigationViewPageProvider : INavigationViewPageProvider
{
    private readonly IServiceProvider _services;

    public NavigationViewPageProvider(IServiceProvider services)
    {
        _services = services;
    }

    public object? GetPage(Type pageType)
    {
        var page = _services.GetService(pageType);
        if (page is not null)
        {
            return page;
        }

        return Activator.CreateInstance(pageType);
    }
}

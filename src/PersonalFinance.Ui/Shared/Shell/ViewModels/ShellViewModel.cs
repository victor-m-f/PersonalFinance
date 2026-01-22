using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Features.Expenses.Views;
using PersonalFinance.Ui.Settings;
using PersonalFinance.Ui.Shared.Shell.Services;

namespace PersonalFinance.Ui.Shared.Shell.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ILogger<ShellViewModel> _logger;
    private bool _isInitialized;

    public ICommand InitializeCommand { get; }
    public ICommand NavigateCommand { get; }
    public ICommand BackCommand { get; }

    public ShellViewModel(AppSettingsStore settingsStore, ILogger<ShellViewModel> logger)
    {
        InitializeCommand = new RelayCommand(OnInitialize);
        NavigateCommand = new RelayCommand<Type?>(OnNavigate);
        BackCommand = new RelayCommand(OnBack);
        _settingsStore = settingsStore;
        _logger = logger;
    }

    private void OnInitialize()
    {
        if (_isInitialized)
        {
            _logger.LogDebug("Shell already initialized");
            return;
        }

        _isInitialized = true;
        _logger.LogInformation("Shell initialization started");

        var settings = _settingsStore.LoadOrDefault();
        ThemeApplier.Apply(settings);

        _logger.LogDebug("Theme applied: {Theme}", settings.Theme);

        var navigated = ShellNavigationService.Instance.Navigate(typeof(ExpensesPage));
        if (!navigated)
        {
            _logger.LogWarning("Failed to navigate to default page");
        }
    }

    private void OnNavigate(Type? pageType)
    {
        if (pageType is null)
        {
            _logger.LogDebug("Navigation requested without page type");
            return;
        }

        _logger.LogInformation("Navigation requested to {PageType}", pageType.Name);
        var navigated = ShellNavigationService.Instance.Navigate(pageType);
        if (!navigated)
        {
            _logger.LogWarning("Navigation failed to {PageType}", pageType.Name);
        }
    }

    private void OnBack()
    {
        if (!ShellNavigationService.Instance.IsBackEnabled)
        {
            _logger.LogDebug("Back navigation requested but not available");
            return;
        }

        _logger.LogInformation("Back navigation requested");
        ShellNavigationService.Instance.GoBack();
    }
}

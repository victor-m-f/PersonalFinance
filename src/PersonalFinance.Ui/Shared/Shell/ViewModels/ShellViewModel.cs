using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Ui.Features.Expenses.Views;
using PersonalFinance.Ui.Settings;
using PersonalFinance.Ui.Shared.Shell.Services;

namespace PersonalFinance.Ui.Shared.Shell.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
    private readonly AppSettingsStore _settingsStore = new();
    private bool _isInitialized;

    public ICommand InitializeCommand { get; }
    public ICommand NavigateCommand { get; }
    public ICommand BackCommand { get; }

    public ShellViewModel()
    {
        InitializeCommand = new RelayCommand(OnInitialize);
        NavigateCommand = new RelayCommand<Type?>(OnNavigate);
        BackCommand = new RelayCommand(OnBack);
    }

    private void OnInitialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        var settings = _settingsStore.LoadOrDefault();
        ThemeApplier.Apply(settings);

        ShellNavigationService.Instance.Navigate(typeof(ExpensesPage));
    }

    private void OnNavigate(Type? pageType)
    {
        if (pageType is null)
        {
            return;
        }

        ShellNavigationService.Instance.Navigate(pageType);
    }

    private void OnBack()
    {
        if (!ShellNavigationService.Instance.IsBackEnabled)
        {
            return;
        }

        ShellNavigationService.Instance.GoBack();
    }
}

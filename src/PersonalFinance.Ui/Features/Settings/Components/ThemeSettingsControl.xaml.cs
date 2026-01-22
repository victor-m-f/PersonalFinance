using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Settings.Components.ViewModels;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.Settings.Components;

public partial class ThemeSettingsControl : UserControl
{
    public ThemeSettingsControl()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ThemeSettingsViewModel>();
    }
}

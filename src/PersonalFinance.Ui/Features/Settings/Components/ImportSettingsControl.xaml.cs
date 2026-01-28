using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Ui.Features.Settings.Components.ViewModels;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Features.Settings.Components;

public partial class ImportSettingsControl : UserControl
{
    public ImportSettingsControl()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ImportSettingsViewModel>();
    }
}

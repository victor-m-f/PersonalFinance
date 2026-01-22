using PersonalFinance.Ui.Helpers;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Settings;

public static class ThemeApplier
{
    public static void Apply(AppSettings settings)
    {
        switch (settings.Theme)
        {
            case AppThemePreference.Dark:
                ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.None);
                break;
            case AppThemePreference.Light:
                ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.None);
                break;
            default:
                var systemTheme = SystemThemeHelper.GetPreferredAppTheme();
                ApplicationThemeManager.Apply(systemTheme, WindowBackdropType.None);
                break;
        }
    }
}

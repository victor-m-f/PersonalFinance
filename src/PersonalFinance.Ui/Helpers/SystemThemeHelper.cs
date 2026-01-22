using System;
using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace PersonalFinance.Ui.Helpers;

public static class SystemThemeHelper
{
    private const string PersonalizeKey =
        "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";

    public static ApplicationTheme GetPreferredAppTheme()
    {
        try
        {
            if (SystemParameters.HighContrast)
            {
                return ApplicationTheme.HighContrast;
            }

            var appsUseLightValue = Registry.GetValue(PersonalizeKey, "AppsUseLightTheme", 1);
            var systemUsesLightValue = Registry.GetValue(PersonalizeKey, "SystemUsesLightTheme", 1);

            var appsUseLight = Convert.ToInt32(appsUseLightValue) != 0;
            var systemUsesLight = Convert.ToInt32(systemUsesLightValue) != 0;

            if (!appsUseLight || !systemUsesLight)
            {
                return ApplicationTheme.Dark;
            }

            return ApplicationTheme.Light;
        }
        catch
        {
            return ApplicationTheme.Light;
        }
    }
}

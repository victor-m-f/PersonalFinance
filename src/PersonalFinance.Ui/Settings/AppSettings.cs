namespace PersonalFinance.Ui.Settings;

public sealed class AppSettings
{
    public AppThemePreference Theme { get; set; } = AppThemePreference.System;
}

public enum AppThemePreference
{
    System,
    Light,
    Dark
}

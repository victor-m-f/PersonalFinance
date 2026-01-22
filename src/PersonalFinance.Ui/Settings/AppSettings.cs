namespace PersonalFinance.Ui.Settings;

public sealed class AppSettings
{
    public AppThemePreference Theme { get; set; } = AppThemePreference.System;
    public string CultureName { get; set; } = "en-US";
    public bool VerboseLogging { get; set; }
}

public enum AppThemePreference
{
    System,
    Light,
    Dark
}

using System;
using System.IO;

namespace PersonalFinance.Ui.Helpers;

public static class FileSystemPaths
{
    public static string LocalAppDataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PersonalFinance"
    );

    public static string SettingsFilePath => Path.Combine(LocalAppDataFolder, "settings.json");
}

using System.IO;
using System.Text.Json;
using PersonalFinance.Ui.Helpers;

namespace PersonalFinance.Ui.Settings;

public sealed class AppSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings LoadOrDefault()
    {
        try
        {
            if (!File.Exists(FileSystemPaths.SettingsFilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(FileSystemPaths.SettingsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppSettings();
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(FileSystemPaths.LocalAppDataFolder);
            var json = JsonSerializer.Serialize(settings, SerializerOptions);
            File.WriteAllText(FileSystemPaths.SettingsFilePath, json);
        }
        catch
        {
            // Ignore persistence errors.
        }
    }
}

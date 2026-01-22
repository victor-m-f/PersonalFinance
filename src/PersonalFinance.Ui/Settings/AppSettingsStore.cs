using System.IO;
using System.Text.Json;
using PersonalFinance.Ui.Helpers;

namespace PersonalFinance.Ui.Settings;

public sealed class AppSettingsStore
{
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-US",
        "pt-BR"
    };

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
            var normalized = settings ?? new AppSettings();
            normalized.CultureName = NormalizeCultureName(normalized.CultureName);
            return normalized;
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
        }
    }

    private static string NormalizeCultureName(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return "en-US";
        }

        return SupportedCultures.Contains(cultureName) ? cultureName : "en-US";
    }
}

using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PersonalFinance.Ui.Helpers;

namespace PersonalFinance.Ui.Settings;

public sealed class AppSettingsStore
{
    private readonly ILogger<AppSettingsStore> _logger;
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

    public AppSettingsStore(ILogger<AppSettingsStore> logger)
    {
        _logger = logger;
    }

    public AppSettings LoadOrDefault()
    {
        try
        {
            if (!File.Exists(FileSystemPaths.SettingsFilePath))
            {
                _logger.LogDebug("Settings file not found");
                return new AppSettings();
            }

            var json = File.ReadAllText(FileSystemPaths.SettingsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("Settings file is empty");
                return new AppSettings();
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions);
            var normalized = settings ?? new AppSettings();
            normalized.CultureName = NormalizeCultureName(normalized.CultureName);
            return normalized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings");
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save settings");
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

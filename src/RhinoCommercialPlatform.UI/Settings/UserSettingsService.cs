using System;
using System.IO;
using System.Text;
using System.Text.Json;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.UI.Settings;

public sealed class UserSettingsService : IUserSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly IAppPaths _paths;
    private readonly IAppLogger _logger;

    public UserSettingsService(IAppPaths paths, IAppLogger logger)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public UserSettings Load()
    {
        var filePath = GetSettingsFilePath();

        if (!File.Exists(filePath))
        {
            _logger.Debug("Settings file not found, returning defaults.");
            return UserSettings.CreateDefaults();
        }

        try
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return ParseSettings(json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error($"Failed to read settings file: {ex.Message}");
            return UserSettings.CreateDefaults();
        }
        catch (JsonException ex)
        {
            _logger.Error($"Failed to parse settings JSON: {ex.Message}");
            return UserSettings.CreateDefaults();
        }
    }

    public void Save(UserSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        settings.UpdatedAtUtc = DateTime.UtcNow;

        var filePath = GetSettingsFilePath();
        var tempFilePath = filePath + ".tmp";
        var dirPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        try
        {
            var json = SerializeSettings(settings);
            var encodedBytes = Encoding.UTF8.GetBytes(json);

            // Atomic write: write to temp, flush, then replace
            using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(encodedBytes, 0, encodedBytes.Length);
                stream.Flush(true);
            }

            // Replace the old file atomically
            File.Replace(tempFilePath, filePath, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error($"Failed to save settings file: {ex.Message}");

            // Clean up temp file if it exists
            try
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            catch
            {
                // Best effort cleanup
            }

            throw;
        }
    }

    public UserSettings ResetToDefaults()
    {
        var defaults = UserSettings.CreateDefaults();
        Save(defaults);
        return defaults;
    }

    private string GetSettingsFilePath()
    {
        return Path.Combine(_paths.ConfigDirectory, UserSettingsSchema.FileName);
    }

    internal static UserSettings ParseSettings(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return UserSettings.CreateDefaults();

        try
        {
            var settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
            if (settings == null)
                return UserSettings.CreateDefaults();

            // Validate and sanitize fields
            SanitizeSettings(settings);
            return settings;
        }
        catch (JsonException)
        {
            return UserSettings.CreateDefaults();
        }
    }

    internal static string SerializeSettings(UserSettings settings)
    {
        return JsonSerializer.Serialize(settings, JsonOptions);
    }

    internal static void SanitizeSettings(UserSettings settings)
    {
        // Validate ThemeMode
        if (!string.IsNullOrEmpty(settings.ThemeMode))
        {
            var valid = false;
            foreach (var mode in UserSettingsDefaults.ValidThemeModes)
            {
                if (string.Equals(settings.ThemeMode, mode, StringComparison.OrdinalIgnoreCase))
                {
                    settings.ThemeMode = mode;
                    valid = true;
                    break;
                }
            }
            if (!valid)
                settings.ThemeMode = UserSettingsDefaults.ThemeMode;
        }
        else
        {
            settings.ThemeMode = UserSettingsDefaults.ThemeMode;
        }

        // Validate LastPage
        if (!string.IsNullOrEmpty(settings.LastPage))
        {
            var valid = false;
            foreach (var page in UserSettingsDefaults.ValidPages)
            {
                if (string.Equals(settings.LastPage, page, StringComparison.OrdinalIgnoreCase))
                {
                    settings.LastPage = page;
                    valid = true;
                    break;
                }
            }
            if (!valid)
                settings.LastPage = UserSettingsDefaults.LastPage;
        }
        else
        {
            settings.LastPage = UserSettingsDefaults.LastPage;
        }

        // Validate LogLevel
        if (!string.IsNullOrEmpty(settings.LogLevel))
        {
            var valid = false;
            foreach (var level in UserSettingsDefaults.ValidLogLevels)
            {
                if (string.Equals(settings.LogLevel, level, StringComparison.OrdinalIgnoreCase))
                {
                    settings.LogLevel = level;
                    valid = true;
                    break;
                }
            }
            if (!valid)
                settings.LogLevel = UserSettingsDefaults.LogLevel;
        }
        else
        {
            settings.LogLevel = UserSettingsDefaults.LogLevel;
        }

        // Default Language
        if (string.IsNullOrWhiteSpace(settings.Language))
            settings.Language = UserSettingsDefaults.Language;

        // Clamp SchemaVersion
        if (settings.SchemaVersion <= 0)
            settings.SchemaVersion = UserSettingsDefaults.SchemaVersion;
    }
}

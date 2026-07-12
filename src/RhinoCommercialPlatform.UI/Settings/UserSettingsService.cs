using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.UI.Settings;

/// <summary>
/// Thread-safe UserSettings service with atomic file persistence.
/// Uses DataContractJsonSerializer to avoid System.Text.Json runtime dependency.
/// </summary>
public sealed class UserSettingsService : IUserSettingsService
{
    private static readonly DataContractJsonSerializer Serializer =
        new DataContractJsonSerializer(typeof(UserSettings),
            new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = false,
                EmitTypeInformation = EmitTypeInformation.Never,
                DateTimeFormat = new DateTimeFormat("o")
            });

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
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length == 0)
            {
                _logger.Warning("Settings file is empty, returning defaults.");
                return UserSettings.CreateDefaults();
            }

            var settings = (UserSettings?)Serializer.ReadObject(stream);
            if (settings == null)
                return UserSettings.CreateDefaults();

            SanitizeSettings(settings);
            return settings;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error($"Failed to read settings file: {ex.Message}");
            return UserSettings.CreateDefaults();
        }
        catch (SerializationException ex)
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
            // Write to temp file atomically
            using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                    stream, Encoding.UTF8, ownsStream: false, indent: true, indentChars: "  "))
                {
                    Serializer.WriteObject(writer, settings);
                    writer.Flush();
                }
                stream.Flush(true);
            }

            // Atomic replacement
            if (File.Exists(filePath))
            {
                // File.Replace atomically replaces the destination with the source
                File.Replace(tempFilePath, filePath, null);
            }
            else
            {
                // First save: move temp file to target
                File.Move(tempFilePath, filePath);
            }

            _logger.Debug("Settings saved successfully.");
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
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var settings = (UserSettings?)Serializer.ReadObject(stream);
            if (settings == null)
                return UserSettings.CreateDefaults();

            SanitizeSettings(settings);
            return settings;
        }
        catch (Exception)
        {
            return UserSettings.CreateDefaults();
        }
    }

    internal static string SerializeSettings(UserSettings settings)
    {
        using var stream = new MemoryStream();
        using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
            stream, Encoding.UTF8, ownsStream: false, indent: true, indentChars: "  "))
        {
            Serializer.WriteObject(writer, settings);
            writer.Flush();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
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

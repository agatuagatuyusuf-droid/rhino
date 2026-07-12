using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.UI.Settings;

/// <summary>
/// Thread-safe UserSettings service with atomic file persistence.
/// Uses DataContractJsonSerializer to avoid System.Text.Json runtime dependency.
///
/// Thread safety: ReaderWriterLockSlim guards Load/Save/Reset concurrency.
/// Temp files use a UUID suffix to avoid multi-process collision.
/// </summary>
public sealed class UserSettingsService : IUserSettingsService
{
    private readonly IAppPaths _paths;
    private readonly IAppLogger _logger;
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    public UserSettingsService(IAppPaths paths, IAppLogger logger)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public UserSettings Load()
    {
        _rwLock.EnterReadLock();
        try
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

                var serializer = CreateSerializer();
                var settings = (UserSettings?)serializer.ReadObject(stream);
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
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public void Save(UserSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        settings.UpdatedAtUtc = DateTime.UtcNow;

        _rwLock.EnterWriteLock();
        try
        {
            var filePath = GetSettingsFilePath();
            SaveCore(settings, filePath);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public UserSettings ResetToDefaults()
    {
        var defaults = UserSettings.CreateDefaults();
        defaults.UpdatedAtUtc = DateTime.UtcNow;

        _rwLock.EnterWriteLock();
        try
        {
            var filePath = GetSettingsFilePath();
            SaveCore(defaults, filePath);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }

        return defaults;
    }

    private void SaveCore(UserSettings settings, string filePath)
    {
        var tempFilePath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var dirPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        try
        {
            var serializer = CreateSerializer();

            using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                    stream, Encoding.UTF8, ownsStream: false, indent: true, indentChars: "  "))
                {
                    serializer.WriteObject(writer, settings);
                    writer.Flush();
                }
                stream.Flush(true);
            }

            if (File.Exists(filePath))
            {
                File.Replace(tempFilePath, filePath, null);
            }
            else
            {
                File.Move(tempFilePath, filePath);
            }

            _logger.Debug("Settings saved successfully.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error($"Failed to save settings file: {ex.Message}");

            try
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            catch
            {
            }

            throw;
        }
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
            var serializer = CreateSerializer();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var settings = (UserSettings?)serializer.ReadObject(stream);
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
        var serializer = CreateSerializer();
        using var stream = new MemoryStream();
        using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
            stream, Encoding.UTF8, ownsStream: false, indent: true, indentChars: "  "))
        {
            serializer.WriteObject(writer, settings);
            writer.Flush();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static DataContractJsonSerializer CreateSerializer()
    {
        return new DataContractJsonSerializer(typeof(UserSettings),
            new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = false,
                EmitTypeInformation = EmitTypeInformation.Never,
                DateTimeFormat = new DateTimeFormat("o")
            });
    }

    internal static void SanitizeSettings(UserSettings settings)
    {
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

        if (string.IsNullOrWhiteSpace(settings.Language))
            settings.Language = UserSettingsDefaults.Language;

        if (settings.SchemaVersion <= 0)
            settings.SchemaVersion = UserSettingsDefaults.SchemaVersion;
    }
}

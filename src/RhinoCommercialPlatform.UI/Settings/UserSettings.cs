using System;
using System.Runtime.Serialization;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Settings;

[DataContract(Name = "settings", Namespace = "")]
public sealed class UserSettings
{
    [DataMember(Name = "schemaVersion", Order = 0)]
    public int SchemaVersion { get; set; } = UserSettingsDefaults.SchemaVersion;

    [DataMember(Name = "themeMode", Order = 1)]
    public string ThemeMode { get; set; } = UserSettingsDefaults.ThemeMode;

    [DataMember(Name = "autoOpenPanel", Order = 2)]
    public bool AutoOpenPanel { get; set; } = UserSettingsDefaults.AutoOpenPanel;

    [DataMember(Name = "rememberLastPage", Order = 3)]
    public bool RememberLastPage { get; set; } = UserSettingsDefaults.RememberLastPage;

    [DataMember(Name = "lastPage", Order = 4)]
    public string LastPage { get; set; } = UserSettingsDefaults.LastPage;

    [DataMember(Name = "logLevel", Order = 5)]
    public string LogLevel { get; set; } = UserSettingsDefaults.LogLevel;

    [DataMember(Name = "language", Order = 6)]
    public string Language { get; set; } = UserSettingsDefaults.Language;

    [DataMember(Name = "updatedAtUtc", Order = 7)]
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Called by DataContractJsonSerializer before deserialization.
    /// Sets all properties to defaults so missing JSON fields don't leave
    /// zero-value defaults (e.g., false for bool, 0 for int).
    /// </summary>
    [OnDeserializing]
    private void OnDeserializing(StreamingContext context)
    {
        SchemaVersion = UserSettingsDefaults.SchemaVersion;
        ThemeMode = UserSettingsDefaults.ThemeMode;
        AutoOpenPanel = UserSettingsDefaults.AutoOpenPanel;
        RememberLastPage = UserSettingsDefaults.RememberLastPage;
        LastPage = UserSettingsDefaults.LastPage;
        LogLevel = UserSettingsDefaults.LogLevel;
        Language = UserSettingsDefaults.Language;
        UpdatedAtUtc = null;
    }

    public static UserSettings CreateDefaults()
    {
        return new UserSettings
        {
            SchemaVersion = UserSettingsDefaults.SchemaVersion,
            ThemeMode = UserSettingsDefaults.ThemeMode,
            AutoOpenPanel = UserSettingsDefaults.AutoOpenPanel,
            RememberLastPage = UserSettingsDefaults.RememberLastPage,
            LastPage = UserSettingsDefaults.LastPage,
            LogLevel = UserSettingsDefaults.LogLevel,
            Language = UserSettingsDefaults.Language,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}

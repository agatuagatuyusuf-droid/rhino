using System;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Settings;

public sealed class UserSettings
{
    public int SchemaVersion { get; set; } = UserSettingsDefaults.SchemaVersion;
    public string ThemeMode { get; set; } = UserSettingsDefaults.ThemeMode;
    public bool AutoOpenPanel { get; set; } = UserSettingsDefaults.AutoOpenPanel;
    public bool RememberLastPage { get; set; } = UserSettingsDefaults.RememberLastPage;
    public string LastPage { get; set; } = UserSettingsDefaults.LastPage;
    public string LogLevel { get; set; } = UserSettingsDefaults.LogLevel;
    public string Language { get; set; } = UserSettingsDefaults.Language;
    public DateTime? UpdatedAtUtc { get; set; }

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

using System;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class SettingsViewModel
{
    private readonly IUserSettingsService _settingsService;
    private readonly ThemeManager _themeManager;

    private readonly UserSettings _currentSettings;

    public event EventHandler? SettingsSaved;
    public event EventHandler? SettingsReset;

    public SettingsViewModel(
        IUserSettingsService settingsService,
        ThemeManager themeManager,
        UserSettings currentSettings)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _currentSettings = currentSettings ?? throw new ArgumentNullException(nameof(currentSettings));
    }

    public UserSettings CurrentSettings => _currentSettings;

    // Theme
    public string SelectedTheme
    {
        get => _currentSettings.ThemeMode;
        set
        {
            if (_currentSettings.ThemeMode != value)
            {
                _currentSettings.ThemeMode = value;
                _themeManager.SetThemeFromModeString(value);
            }
        }
    }

    // Auto open
    public bool AutoOpenPanel
    {
        get => _currentSettings.AutoOpenPanel;
        set => _currentSettings.AutoOpenPanel = value;
    }

    // Remember last page
    public bool RememberLastPage
    {
        get => _currentSettings.RememberLastPage;
        set => _currentSettings.RememberLastPage = value;
    }

    // Log level
    public string SelectedLogLevel
    {
        get => _currentSettings.LogLevel;
        set => _currentSettings.LogLevel = value;
    }

    public void Save()
    {
        _settingsService.Save(_currentSettings);
        SettingsSaved?.Invoke(this, EventArgs.Empty);
    }

    public void ResetToDefaults()
    {
        CopySettings(_settingsService.ResetToDefaults());
        _themeManager.SetThemeFromModeString(_currentSettings.ThemeMode);
        SettingsReset?.Invoke(this, EventArgs.Empty);
    }

    public void Reload()
    {
        CopySettings(_settingsService.Load());
    }

    private void CopySettings(UserSettings settings)
    {
        _currentSettings.SchemaVersion = settings.SchemaVersion;
        _currentSettings.ThemeMode = settings.ThemeMode;
        _currentSettings.AutoOpenPanel = settings.AutoOpenPanel;
        _currentSettings.RememberLastPage = settings.RememberLastPage;
        _currentSettings.LastPage = settings.LastPage;
        _currentSettings.LogLevel = settings.LogLevel;
        _currentSettings.Language = settings.Language;
        _currentSettings.UpdatedAtUtc = settings.UpdatedAtUtc;
    }
}

using System;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class SettingsViewModel
{
    private readonly IUserSettingsService _settingsService;
    private readonly ThemeManager _themeManager;

    private UserSettings _currentSettings;

    public event EventHandler? SettingsSaved;
    public event EventHandler? SettingsReset;

    public SettingsViewModel(IUserSettingsService settingsService, ThemeManager themeManager)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _currentSettings = _settingsService.Load();
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
        _currentSettings = _settingsService.ResetToDefaults();
        _themeManager.SetThemeFromModeString(_currentSettings.ThemeMode);
        SettingsReset?.Invoke(this, EventArgs.Empty);
    }

    public void Reload()
    {
        _currentSettings = _settingsService.Load();
    }
}

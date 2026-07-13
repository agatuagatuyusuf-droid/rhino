using System;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.UI.Theme;

public sealed class ThemeManager
{
    private readonly ISystemThemeProvider? _systemThemeProvider;
    private ThemeMode _currentMode = ThemeMode.System;
    private ThemePalette? _currentPalette;

    public event EventHandler<ThemePalette>? ThemeChanged;

    public ThemeMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode == value)
                return;

            _currentMode = value;
            ApplyTheme();
        }
    }

    public ThemePalette CurrentPalette
    {
        get
        {
            if (_currentPalette == null)
                ApplyTheme();
            return _currentPalette!;
        }
    }

    public ThemeManager(ISystemThemeProvider? systemThemeProvider = null)
    {
        _systemThemeProvider = systemThemeProvider;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        ThemePalette palette;

        switch (_currentMode)
        {
            case ThemeMode.Light:
                palette = ThemePalette.CreateLight();
                break;
            case ThemeMode.Dark:
                palette = ThemePalette.CreateDark();
                break;
            case ThemeMode.System:
            default:
                palette = GetSystemPalette();
                break;
        }

        _currentPalette = palette;
        ThemeChanged?.Invoke(this, palette);
    }

    public void SetThemeFromModeString(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            CurrentMode = ThemeMode.System;
            return;
        }

        switch (mode.ToLowerInvariant())
        {
            case "light":
                CurrentMode = ThemeMode.Light;
                break;
            case "dark":
                CurrentMode = ThemeMode.Dark;
                break;
            case "system":
            default:
                CurrentMode = ThemeMode.System;
                break;
        }
    }

    public string GetModeString()
    {
        return _currentMode switch
        {
            ThemeMode.Light => "Light",
            ThemeMode.Dark => "Dark",
            _ => "System"
        };
    }

    private ThemePalette GetSystemPalette()
    {
        if (_systemThemeProvider != null)
        {
            var systemTheme = _systemThemeProvider.GetCurrentSystemTheme();
            if (systemTheme == SystemTheme.Dark)
                return ThemePalette.CreateDark();
        }

        return ThemePalette.CreateLight();
    }
}

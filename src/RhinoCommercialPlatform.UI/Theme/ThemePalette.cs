using Eto.Drawing;

namespace RhinoCommercialPlatform.UI.Theme;

public sealed class ThemePalette
{
    // Backgrounds
    public Color Background { get; }
    public Color SidebarBackground { get; }
    public Color Surface { get; }
    public Color SurfaceElevated { get; }
    public Color Border { get; }
    public Color HeaderBackground { get; }
    public Color StatusBarBackground { get; }

    // Text
    public Color TextPrimary { get; }
    public Color TextSecondary { get; }
    public Color TextMuted { get; }

    // Interactive
    public Color Accent { get; }
    public Color AccentText { get; }
    public Color Hover { get; }
    public Color Selected { get; }
    public Color Pressed { get; }

    // Status
    public Color Success { get; }
    public Color Warning { get; }
    public Color Error { get; }
    public Color Info { get; }

    // Cards
    public Color CardBackground { get; }
    public Color CardBorder { get; }

    // Navigation
    public Color NavItemHover { get; }
    public Color NavItemSelected { get; }
    public Color NavItemText { get; }
    public Color NavItemSelectedText { get; }

    // Buttons
    public Color ButtonBackground { get; }
    public Color ButtonText { get; }
    public Color ButtonHover { get; }

    public bool IsDark { get; }

    private ThemePalette(
        Color background,
        Color sidebarBackground,
        Color surface,
        Color surfaceElevated,
        Color border,
        Color headerBackground,
        Color statusBarBackground,
        Color textPrimary,
        Color textSecondary,
        Color textMuted,
        Color accent,
        Color accentText,
        Color hover,
        Color selected,
        Color pressed,
        Color success,
        Color warning,
        Color error,
        Color info,
        Color cardBackground,
        Color cardBorder,
        Color navItemHover,
        Color navItemSelected,
        Color navItemText,
        Color navItemSelectedText,
        Color buttonBackground,
        Color buttonText,
        Color buttonHover,
        bool isDark)
    {
        Background = background;
        SidebarBackground = sidebarBackground;
        Surface = surface;
        SurfaceElevated = surfaceElevated;
        Border = border;
        HeaderBackground = headerBackground;
        StatusBarBackground = statusBarBackground;
        TextPrimary = textPrimary;
        TextSecondary = textSecondary;
        TextMuted = textMuted;
        Accent = accent;
        AccentText = accentText;
        Hover = hover;
        Selected = selected;
        Pressed = pressed;
        Success = success;
        Warning = warning;
        Error = error;
        Info = info;
        CardBackground = cardBackground;
        CardBorder = cardBorder;
        NavItemHover = navItemHover;
        NavItemSelected = navItemSelected;
        NavItemText = navItemText;
        NavItemSelectedText = navItemSelectedText;
        ButtonBackground = buttonBackground;
        ButtonText = buttonText;
        ButtonHover = buttonHover;
        IsDark = isDark;
    }

    public static ThemePalette CreateLight()
    {
        return new ThemePalette(
            background: Color.FromArgb(0xF5, 0xF5, 0xF7),
            sidebarBackground: Color.FromArgb(0xEC, 0xEC, 0xEE),
            surface: Color.FromArgb(0xFF, 0xFF, 0xFF),
            surfaceElevated: Color.FromArgb(0xFF, 0xFF, 0xFF),
            border: Color.FromArgb(0xD2, 0xD2, 0xD7),
            headerBackground: Color.FromArgb(0xE8, 0xE8, 0xEA),
            statusBarBackground: Color.FromArgb(0xEC, 0xEC, 0xEE),
            textPrimary: Color.FromArgb(0x1C, 0x1C, 0x1E),
            textSecondary: Color.FromArgb(0x63, 0x63, 0x67),
            textMuted: Color.FromArgb(0x8E, 0x8E, 0x93),
            accent: Color.FromArgb(0x00, 0x7A, 0xFF),
            accentText: Colors.White,
            hover: Color.FromArgb(0xE8, 0xE8, 0xEA),
            selected: Color.FromArgb(0xD0, 0xD0, 0xD5),
            pressed: Color.FromArgb(0xBC, 0xBC, 0xC2),
            success: Color.FromArgb(0x34, 0xC7, 0x59),
            warning: Color.FromArgb(0xFF, 0x9F, 0x0A),
            error: Color.FromArgb(0xFF, 0x3B, 0x30),
            info: Color.FromArgb(0x00, 0x7A, 0xFF),
            cardBackground: Color.FromArgb(0xFF, 0xFF, 0xFF),
            cardBorder: Color.FromArgb(0xE5, 0xE5, 0xEA),
            navItemHover: Color.FromArgb(0xD8, 0xD8, 0xDA),
            navItemSelected: Color.FromArgb(0xC0, 0xC0, 0xC5),
            navItemText: Color.FromArgb(0x3A, 0x3A, 0x3C),
            navItemSelectedText: Color.FromArgb(0x1C, 0x1C, 0x1E),
            buttonBackground: Color.FromArgb(0xE8, 0xE8, 0xEA),
            buttonText: Color.FromArgb(0x1C, 0x1C, 0x1E),
            buttonHover: Color.FromArgb(0xD8, 0xD8, 0xDA),
            isDark: false
        );
    }

    public static ThemePalette CreateDark()
    {
        return new ThemePalette(
            background: Color.FromArgb(0x1C, 0x1C, 0x1E),
            sidebarBackground: Color.FromArgb(0x25, 0x25, 0x27),
            surface: Color.FromArgb(0x2C, 0x2C, 0x2E),
            surfaceElevated: Color.FromArgb(0x36, 0x36, 0x38),
            border: Color.FromArgb(0x44, 0x44, 0x46),
            headerBackground: Color.FromArgb(0x25, 0x25, 0x27),
            statusBarBackground: Color.FromArgb(0x25, 0x25, 0x27),
            textPrimary: Color.FromArgb(0xF5, 0xF5, 0xF7),
            textSecondary: Color.FromArgb(0xA8, 0xA8, 0xAD),
            textMuted: Color.FromArgb(0x8E, 0x8E, 0x93),
            accent: Color.FromArgb(0x0A, 0x84, 0xFF),
            accentText: Colors.White,
            hover: Color.FromArgb(0x3A, 0x3A, 0x3C),
            selected: Color.FromArgb(0x48, 0x48, 0x4A),
            pressed: Color.FromArgb(0x55, 0x55, 0x57),
            success: Color.FromArgb(0x30, 0xD1, 0x58),
            warning: Color.FromArgb(0xFF, 0x9F, 0x0A),
            error: Color.FromArgb(0xFF, 0x45, 0x3A),
            info: Color.FromArgb(0x0A, 0x84, 0xFF),
            cardBackground: Color.FromArgb(0x2C, 0x2C, 0x2E),
            cardBorder: Color.FromArgb(0x44, 0x44, 0x46),
            navItemHover: Color.FromArgb(0x3A, 0x3A, 0x3C),
            navItemSelected: Color.FromArgb(0x48, 0x48, 0x4A),
            navItemText: Color.FromArgb(0xA8, 0xA8, 0xAD),
            navItemSelectedText: Color.FromArgb(0xF5, 0xF5, 0xF7),
            buttonBackground: Color.FromArgb(0x3A, 0x3A, 0x3C),
            buttonText: Color.FromArgb(0xF5, 0xF5, 0xF7),
            buttonHover: Color.FromArgb(0x48, 0x48, 0x4A),
            isDark: true
        );
    }
}

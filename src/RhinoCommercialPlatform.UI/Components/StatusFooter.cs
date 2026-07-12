using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Components;

public sealed class StatusFooter : Panel
{
    private readonly Label _statusLabel;
    private readonly Label _platformLabel;
    private readonly Label _versionLabel;
    private readonly DynamicLayout _layout;
    private ThemePalette? _currentPalette;

    public string StatusText
    {
        get => _statusLabel.Text;
        set => _statusLabel.Text = value;
    }

    public string PlatformText
    {
        get => _platformLabel.Text;
        set => _platformLabel.Text = value;
    }

    public string VersionText
    {
        get => _versionLabel.Text;
        set => _versionLabel.Text = value;
    }

    public StatusFooter()
    {
        _statusLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Height = UiTokens.StatusBarHeight
        };

        _platformLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Height = UiTokens.StatusBarHeight
        };

        _versionLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Height = UiTokens.StatusBarHeight
        };

        _layout = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium, 2),
            Spacing = new Size(UiTokens.SpacingMedium, 0),
            Height = UiTokens.StatusBarHeight
        };

        _layout.AddRow(_statusLabel, _platformLabel, _versionLabel);

        Content = _layout;
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _currentPalette = palette;
        _layout.BackgroundColor = palette.StatusBarBackground;
        _statusLabel.TextColor = palette.TextMuted;
        _platformLabel.TextColor = palette.TextMuted;
        _versionLabel.TextColor = palette.TextMuted;
    }
}

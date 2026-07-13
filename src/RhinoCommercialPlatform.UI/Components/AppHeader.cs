using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Components;

public sealed class AppHeader : Panel
{
    private readonly Label _titleLabel;
    private readonly DynamicLayout _layout;
    private ThemePalette? _currentPalette;

    public event EventHandler? ThemeButtonClicked;
    public event EventHandler? SettingsButtonClicked;

    public AppHeader()
    {
        _titleLabel = new Label
        {
            Text = "RhinoCommercialPlatform",
            Font = SystemFonts.Bold(UiTokens.FontSizeTitle),
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Wrap = WrapMode.None
        };

        var themeButton = new Button
        {
            Text = "🌓",
            ToolTip = "切换主题",
            Size = new Size(32, 32)
        };
        themeButton.Click += (_, _) => ThemeButtonClicked?.Invoke(this, EventArgs.Empty);

        var settingsButton = new Button
        {
            Text = "⚙",
            ToolTip = "设置",
            Size = new Size(32, 32)
        };
        settingsButton.Click += (_, _) => SettingsButtonClicked?.Invoke(this, EventArgs.Empty);

        var buttonPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = UiTokens.SpacingSmall,
            Items = { themeButton, settingsButton }
        };

        _layout = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium, UiTokens.SpacingSmall),
            Spacing = new Size(UiTokens.SpacingMedium, 0),
            Height = UiTokens.HeaderHeight
        };

        _layout.AddRow(_titleLabel, null, buttonPanel);

        Content = _layout;
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _currentPalette = palette;
        _layout.BackgroundColor = palette.HeaderBackground;
        _titleLabel.TextColor = palette.TextPrimary;
    }
}

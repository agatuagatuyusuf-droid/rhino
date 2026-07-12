using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Components;

public sealed class InfoCard : Panel
{
    private readonly DynamicLayout _layout;
    private readonly Label _titleLabel;
    private readonly Label _valueLabel;
    private ThemePalette? _currentPalette;

    public string Title
    {
        get => _titleLabel.Text;
        set => _titleLabel.Text = value;
    }

    public string Value
    {
        get => _valueLabel.Text;
        set => _valueLabel.Text = value;
    }

    public InfoCard()
    {
        _titleLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Wrap = WrapMode.None,
            Height = 16
        };

        _valueLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeBody),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Wrap = WrapMode.Word
        };

        _layout = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(UiTokens.SpacingSmall, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        _layout.Add(_titleLabel);
        _layout.Add(_valueLabel);

        Content = _layout;
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _currentPalette = palette;
        _layout.BackgroundColor = palette.CardBackground;
        _titleLabel.TextColor = palette.TextSecondary;
        _valueLabel.TextColor = palette.TextPrimary;
    }
}

using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Components;

public sealed class EmptyStateView : Panel
{
    private readonly Label _messageLabel;
    private readonly Label _descriptionLabel;
    private readonly StackLayout _stack;

    public string Message
    {
        get => _messageLabel.Text;
        set => _messageLabel.Text = value;
    }

    public string Description
    {
        get => _descriptionLabel.Text;
        set => _descriptionLabel.Text = value;
    }

    public EmptyStateView()
    {
        _messageLabel = new Label
        {
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextAlignment = TextAlignment.Center,
            TextColor = Colors.Gray,
            Height = 24
        };

        _descriptionLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextAlignment = TextAlignment.Center,
            TextColor = Colors.Gray,
            Wrap = WrapMode.Word
        };

        _stack = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = UiTokens.SpacingMedium,
            Padding = new Padding(UiTokens.SpacingLarge),
            Items =
            {
                _messageLabel,
                _descriptionLabel
            }
        };

        Content = _stack;
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _messageLabel.TextColor = palette.TextSecondary;
        _descriptionLabel.TextColor = palette.TextMuted;
    }
}

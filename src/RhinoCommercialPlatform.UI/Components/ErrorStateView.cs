using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Components;

public sealed class ErrorStateView : Panel
{
    private readonly Label _errorMessageLabel;
    private readonly Label _errorDetailLabel;
    private readonly StackLayout _stack;

    public string ErrorMessage
    {
        get => _errorMessageLabel.Text;
        set => _errorMessageLabel.Text = value;
    }

    public string ErrorDetail
    {
        get => _errorDetailLabel.Text;
        set => _errorDetailLabel.Text = value;
    }

    public ErrorStateView()
    {
        _errorMessageLabel = new Label
        {
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextAlignment = TextAlignment.Center,
            TextColor = Colors.Red,
            Height = 24
        };

        _errorDetailLabel = new Label
        {
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextAlignment = TextAlignment.Center,
            TextColor = Colors.DarkRed,
            Wrap = WrapMode.Word
        };

        _stack = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = UiTokens.SpacingMedium,
            Padding = new Padding(UiTokens.SpacingLarge),
            Items =
            {
                _errorMessageLabel,
                _errorDetailLabel
            }
        };

        Content = _stack;
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _errorMessageLabel.TextColor = palette.Error;
        _errorDetailLabel.TextColor = palette.TextSecondary;
    }
}

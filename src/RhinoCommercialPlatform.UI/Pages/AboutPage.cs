using System;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Pages;

public sealed class AboutPage : Panel
{
    private readonly AboutViewModel _viewModel;
    private readonly DynamicLayout _layout;
    private readonly Scrollable _scrollable;
    private ThemePalette? _currentPalette;

    public AboutPage(AboutViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        _layout = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingLarge),
            Spacing = new Size(0, UiTokens.SpacingMedium)
        };

        _scrollable = new Scrollable
        {
            Content = _layout,
            Border = BorderType.None
        };

        BuildContent();

        Content = _scrollable;
    }

    private void BuildContent()
    {
        // Title
        var titleLabel = new Label
        {
            Text = "关于",
            Font = SystemFonts.Bold(UiTokens.FontSizeLargeTitle),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left
        };
        _layout.Add(titleLabel);

        // Product info card
        AddInfoSection(_viewModel.ProductName, new[]
        {
            ("产品版本", _viewModel.ProductVersion),
            ("公司", _viewModel.Company),
            ("项目状态", _viewModel.ProjectStatus)
        });

        // Platform card
        AddInfoSection("平台信息", new[]
        {
            ("当前平台", _viewModel.Platform),
            ("当前架构", _viewModel.Architecture),
            ("运行模式", _viewModel.RunMode),
            ("Rhino 版本", _viewModel.RhinoVersion),
            (".NET 运行时", _viewModel.RuntimeVersion)
        });

        // Build info
        AddInfoSection("构建信息", new[]
        {
            ("Build Commit", _viewModel.BuildCommit),
            ("开发版本", _viewModel.IsDevelopmentBuild ? "是" : "否")
        });

        // License section
        AddInfoSection("授权信息", new[]
        {
            ("授权状态", _viewModel.LicenseStatus),
            ("激活状态", _viewModel.ActivationStatus)
        });

        // Copyright
        var copyrightSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            BackgroundColor = Colors.White
        };

        var copyrightLabel = new Label
        {
            Text = _viewModel.Copyright,
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Center
        };
        copyrightSection.Add(copyrightLabel);
        _layout.Add(copyrightSection);
    }

    private void AddInfoSection(string title, (string label, string value)[] items)
    {
        var section = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(0, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        section.Add(titleLabel);

        foreach (var (label, value) in items)
        {
            var row = new DynamicLayout
            {
                Padding = new Padding(0, 2),
                Spacing = new Size(UiTokens.SpacingMedium, 0)
            };

            var labelCtrl = new Label
            {
                Text = label,
                Font = SystemFonts.Default(UiTokens.FontSizeSmall),
                TextColor = Colors.Gray,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Center
            };

            string displayValue = string.IsNullOrEmpty(value) ? "[无数据]" : value;

            var valueCtrl = new Label
            {
                Text = displayValue,
                Font = SystemFonts.Default(UiTokens.FontSizeBody),
                TextColor = Colors.Black,
                Wrap = WrapMode.Word
            };

            row.AddRow(labelCtrl, valueCtrl);
            section.Add(row);
        }

        _layout.Add(section);
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _currentPalette = palette;
        _layout.BackgroundColor = palette.Background;
        _scrollable.BackgroundColor = palette.Background;

        ApplyThemeToChildren(_layout, palette);
    }

    private void ApplyThemeToChildren(DynamicLayout parent, ThemePalette palette)
    {
        foreach (var child in parent.Controls)
        {
            if (child is Label label)
            {
                if (label.Font == SystemFonts.Bold(UiTokens.FontSizeLargeTitle) ||
                    label.Font == SystemFonts.Bold(UiTokens.FontSizeHeading))
                    label.TextColor = palette.TextPrimary;
                else if (label.TextColor == Colors.Gray)
                    label.TextColor = palette.TextSecondary;
                else
                    label.TextColor = palette.TextPrimary;
            }
            else if (child is DynamicLayout dl)
            {
                dl.BackgroundColor = palette.Surface;
                ApplyThemeToChildren(dl, palette);
            }
        }
    }
}

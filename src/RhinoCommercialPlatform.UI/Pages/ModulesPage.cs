using System;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Components;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Pages;

public sealed class ModulesPage : Panel
{
    private readonly ModulesViewModel _viewModel;
    private readonly DynamicLayout _layout;
    private readonly Scrollable _scrollable;
    private ThemePalette? _currentPalette;

    public ModulesPage(ModulesViewModel viewModel)
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
        var titleLabel = new Label
        {
            Text = "功能模块",
            Font = SystemFonts.Bold(UiTokens.FontSizeLargeTitle),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left
        };
        _layout.Add(titleLabel);

        var description = new Label
        {
            Text = "管理和查看已安装的功能模块。",
            Font = SystemFonts.Default(UiTokens.FontSizeBody),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Left,
            Wrap = WrapMode.Word
        };
        _layout.Add(description);

        var descriptors = _viewModel.Descriptors;
        if (descriptors.Count == 0)
        {
            var emptyView = new EmptyStateView
            {
                Message = "暂无模块",
                Description = "没有已注册的功能模块。"
            };
            _layout.Add(emptyView);
        }
        else
        {
            foreach (var descriptor in descriptors)
            {
                var card = CreateModuleCard(descriptor);
                _layout.Add(card);
            }
        }
    }

    private DynamicLayout CreateModuleCard(ModuleDescriptor descriptor)
    {
        var card = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(0, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var headerRow = new DynamicLayout
        {
            Spacing = new Size(UiTokens.SpacingMedium, 0)
        };

        var nameLabel = new Label
        {
            Text = descriptor.DisplayName,
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black,
            VerticalAlignment = VerticalAlignment.Center
        };

        var statusLabel = new Label
        {
            Text = descriptor.StatusText,
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = descriptor.IsInitialized ? Colors.Green : Colors.Red,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right
        };

        headerRow.AddRow(nameLabel, statusLabel);
        card.Add(headerRow);

        if (!string.IsNullOrEmpty(descriptor.Description))
        {
            var descLabel = new Label
            {
                Text = descriptor.Description,
                Font = SystemFonts.Default(UiTokens.FontSizeSmall),
                TextColor = Colors.Gray,
                Wrap = WrapMode.Word
            };
            card.Add(descLabel);
        }

        var metaRow = new DynamicLayout
        {
            Spacing = new Size(UiTokens.SpacingMedium, 0)
        };

        var idLabel = new Label
        {
            Text = $"ID: {descriptor.Id}",
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            VerticalAlignment = VerticalAlignment.Center
        };

        var versionLabel = new Label
        {
            Text = $"v{descriptor.Version}",
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right
        };

        metaRow.AddRow(idLabel, versionLabel);
        card.Add(metaRow);

        if (!string.IsNullOrEmpty(descriptor.Category))
        {
            var catLabel = new Label
            {
                Text = $"分类: {descriptor.Category}",
                Font = SystemFonts.Default(UiTokens.FontSizeSmall),
                TextColor = Colors.Gray
            };
            card.Add(catLabel);
        }

        return card;
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
                if (label.Font == SystemFonts.Bold(UiTokens.FontSizeLargeTitle))
                    label.TextColor = palette.TextPrimary;
                else if (label.TextColor == Colors.Gray || label.TextColor.R > 0.5)
                    label.TextColor = palette.TextSecondary;
                else if (label.TextColor == Colors.Green)
                    label.TextColor = palette.Success;
                else if (label.TextColor == Colors.Red)
                    label.TextColor = palette.Error;
                else
                    label.TextColor = palette.TextPrimary;
            }
            else if (child is DynamicLayout dl)
            {
                dl.BackgroundColor = palette.CardBackground;
                ApplyThemeToChildren(dl, palette);
            }
            else if (child is EmptyStateView esv)
            {
                esv.ApplyTheme(palette);
            }
        }
    }
}

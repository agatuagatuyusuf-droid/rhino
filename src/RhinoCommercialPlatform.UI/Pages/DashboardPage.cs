using System;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Components;
using RhinoCommercialPlatform.UI.Shell;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Pages;

public sealed class DashboardPage : Panel
{
    private readonly DashboardViewModel _viewModel;
    private readonly DynamicLayout _layout;
    private readonly Scrollable _scrollable;
    private ThemePalette? _currentPalette;

    public event EventHandler<NavigationPageId>? NavigationRequested;
    public event EventHandler? OpenLogDirectoryRequested;

    public DashboardPage(DashboardViewModel viewModel)
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
            Text = "首页",
            Font = SystemFonts.Bold(UiTokens.FontSizeLargeTitle),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left
        };
        _layout.Add(titleLabel);

        // Product info section
        AddInfoSection("产品信息", new[]
        {
            ("产品名称", _viewModel.ProductName),
            ("插件版本", _viewModel.PluginVersion),
            ("运行模式", _viewModel.RunMode),
            ("运行状态", _viewModel.PluginStatus)
        });

        // Platform section
        AddInfoSection("平台信息", new[]
        {
            ("操作系统", $"{_viewModel.OperatingSystem} - {_viewModel.OperatingSystemDescription}"),
            ("进程架构", _viewModel.ProcessArchitecture),
            (".NET 运行时", _viewModel.FrameworkDescription)
        });

        // Module section
        AddInfoSection("模块状态", new[]
        {
            ("已注册模块", _viewModel.RegisteredModuleCount.ToString()),
            ("已初始化模块", _viewModel.InitializedModuleCount.ToString())
        });

        // Path section
        AddInfoSection("路径信息", new[]
        {
            ("日志目录", _viewModel.LogsDirectory),
            ("配置目录", _viewModel.ConfigDirectory)
        });

        // Current time
        AddInfoSection("系统信息", new[]
        {
            ("当前时间", _viewModel.CurrentTime)
        });

        // Recent error
        var lastRuntimeError = _viewModel.LastRuntimeError;
        if (!string.IsNullOrEmpty(lastRuntimeError))
        {
            AddInfoSection("最近错误", new[]
            {
                ("错误摘要", lastRuntimeError!)
            });
        }

        // Quick entries
        var quickSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(UiTokens.SpacingMedium, UiTokens.SpacingMedium),
            BackgroundColor = Colors.White
        };

        var quickTitle = new Label
        {
            Text = "快速入口",
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        quickSection.Add(quickTitle);

        var entryButtons = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = UiTokens.SpacingMedium
        };

        foreach (var entry in _viewModel.QuickEntries)
        {
            var button = new Button
            {
                Text = entry.Title,
                ToolTip = entry.Description,
                Size = new Size(100, 36)
            };

            if (entry.TargetPage.HasValue)
            {
                var targetPage = entry.TargetPage.Value;
                button.Click += (_, _) => NavigationRequested?.Invoke(this, targetPage);
            }
            else
            {
                button.Click += (_, _) => OpenLogDirectoryRequested?.Invoke(this, EventArgs.Empty);
            }

            entryButtons.Items.Add(button);
        }

        quickSection.Add(entryButtons);
        _layout.Add(quickSection);
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

            string displayValue = value;
            if (string.IsNullOrEmpty(displayValue))
                displayValue = "[无数据]";

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

        ApplyThemeToAllChildren(_layout, palette);
    }

    private void ApplyThemeToAllChildren(DynamicLayout parent, ThemePalette palette)
    {
        foreach (var child in parent.Controls)
        {
            if (child is Label label)
            {
                if (label.Font == SystemFonts.Bold(UiTokens.FontSizeLargeTitle) ||
                    label.Font == SystemFonts.Bold(UiTokens.FontSizeHeading))
                {
                    label.TextColor = palette.TextPrimary;
                }
                else if (label.TextColor == Colors.Gray)
                {
                    label.TextColor = palette.TextSecondary;
                }
                else
                {
                    label.TextColor = palette.TextPrimary;
                }
            }
            else if (child is DynamicLayout dl)
            {
                dl.BackgroundColor = palette.Surface;
                ApplyThemeToAllChildren(dl, palette);
            }
            else if (child is StackLayout sl)
            {
                ApplyThemeToStackLayout(sl, palette);
            }
        }
    }

    private void ApplyThemeToStackLayout(StackLayout layout, ThemePalette palette)
    {
        foreach (var item in layout.Items)
        {
            if (item.Control is Label label)
            {
                label.TextColor = palette.TextPrimary;
            }
        }
    }
}

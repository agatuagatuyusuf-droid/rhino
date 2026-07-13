using System;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Components;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Pages;

public sealed class RuntimeStatusPage : Panel
{
    private readonly RuntimeStatusViewModel _viewModel;
    private readonly DynamicLayout _layout;
    private readonly Scrollable _scrollable;
    private ThemePalette? _currentPalette;

    public RuntimeStatusPage(RuntimeStatusViewModel viewModel)
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
            Text = "运行状态",
            Font = SystemFonts.Bold(UiTokens.FontSizeLargeTitle),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left
        };
        _layout.Add(titleLabel);

        // Runtime status
        AddInfoSection("运行时状态", new[]
        {
            ("已启动", _viewModel.IsRuntimeStarted ? "是" : "否"),
            ("启动时间", _viewModel.RuntimeStartedAt),
            ("运行时长", _viewModel.RuntimeUptime)
        });

        // Module section
        AddInfoSection("模块信息", new[]
        {
            ("已注册模块", _viewModel.RegisteredModuleCount.ToString()),
            ("已初始化模块", _viewModel.InitializedModuleCount.ToString())
        });

        // Path section
        AddInfoSection("路径", new[]
        {
            ("日志文件", _viewModel.LogsDirectory),
            ("配置文件", _viewModel.ConfigDirectory)
        });

        // Rhino document section
        if (_viewModel.HasDocument)
        {
            AddInfoSection("Rhino 文档", new[]
            {
                ("文档名称", _viewModel.DocumentName),
                ("文档路径", _viewModel.DocumentPath),
                ("对象数量", _viewModel.DocumentObjectCount)
            });
        }
        else
        {
            AddInfoSection("Rhino 文档", new[]
            {
                ("文档状态", "没有打开文档")
            });
        }

        // Version info
        AddInfoSection("版本信息", new[]
        {
            ("Rhino 版本", _viewModel.RhinoVersion),
            ("插件版本", _viewModel.PluginVersion)
        });

        // Last error
        var lastError = _viewModel.LastError;
        if (!string.IsNullOrEmpty(lastError))
        {
            AddInfoSection("最近错误", new[]
            {
                ("错误", lastError!)
            });
        }
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
                if (label.Font == SystemFonts.Bold(UiTokens.FontSizeLargeTitle))
                    label.TextColor = palette.TextPrimary;
                else if (label.Font == SystemFonts.Bold(UiTokens.FontSizeHeading))
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

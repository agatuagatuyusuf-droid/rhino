using System;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Components;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Pages;

public sealed class DiagnosticsPage : Panel
{
    private readonly DiagnosticsViewModel _viewModel;
    private readonly DynamicLayout _layout;
    private readonly Scrollable _scrollable;
    private readonly ListBox _logListBox;
    private readonly TextArea _diagnosticInfoArea;
    private ThemePalette? _currentPalette;

    public event EventHandler? OpenLogDirectoryRequested;
    public event EventHandler? CopyDiagnosticsRequested;

    public DiagnosticsPage(DiagnosticsViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        _logListBox = new ListBox
        {
            Height = 150,
            BackgroundColor = Colors.White
        };

        _diagnosticInfoArea = new TextArea
        {
            ReadOnly = true,
            Height = 100,
            Wrap = true,
            BackgroundColor = Colors.White,
            Font = SystemFonts.Default(UiTokens.FontSizeSmall)
        };

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
            Text = "诊断中心",
            Font = SystemFonts.Bold(UiTokens.FontSizeLargeTitle),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left
        };
        _layout.Add(titleLabel);

        // Log path
        var logPathSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(UiTokens.SpacingMedium, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var logPathTitle = new Label
        {
            Text = "日志路径",
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        logPathSection.Add(logPathTitle);

        var logPathRow = new DynamicLayout
        {
            Spacing = new Size(UiTokens.SpacingMedium, 0)
        };

        var logPathLabel = new Label
        {
            Text = _viewModel.LogDirectory,
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            VerticalAlignment = VerticalAlignment.Center,
            Wrap = WrapMode.Word
        };

        var openDirBtn = new Button
        {
            Text = "打开目录",
            Size = new Size(90, 28)
        };
        openDirBtn.Click += (_, _) => OpenLogDirectoryRequested?.Invoke(this, EventArgs.Empty);

        var copyPathBtn = new Button
        {
            Text = "复制路径",
            Size = new Size(90, 28)
        };
        copyPathBtn.Click += (_, _) =>
        {
            Eto.Forms.Clipboard.Instance.Text = _viewModel.LogDirectory;
        };

        var buttonsRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = UiTokens.SpacingSmall,
            Items = { openDirBtn, copyPathBtn }
        };

        logPathRow.AddRow(logPathLabel, buttonsRow);
        logPathSection.Add(logPathRow);
        _layout.Add(logPathSection);

        // Recent logs section
        var logSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(0, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var logSectionTitle = new Label
        {
            Text = "最近日志",
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        logSection.Add(logSectionTitle);

        var refreshLogBtn = new Button
        {
            Text = "刷新日志",
            Size = new Size(100, 28)
        };
        refreshLogBtn.Click += (_, _) => RefreshLogs();
        logSection.Add(refreshLogBtn);

        logSection.Add(_logListBox);
        _layout.Add(logSection);

        // Diagnostic info section
        var diagSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(0, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var diagTitle = new Label
        {
            Text = "诊断信息",
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        diagSection.Add(diagTitle);

        var diagButtons = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = UiTokens.SpacingSmall
        };

        var copyDiagBtn = new Button
        {
            Text = "复制诊断信息",
            Size = new Size(120, 28)
        };
        copyDiagBtn.Click += (_, _) => CopyDiagnosticsRequested?.Invoke(this, EventArgs.Empty);
        diagButtons.Items.Add(copyDiagBtn);

        var exportBtn = new Button
        {
            Text = "导出诊断报告",
            Size = new Size(120, 28)
        };
        exportBtn.Click += (_, _) => ExportReport();
        diagButtons.Items.Add(exportBtn);

        diagSection.Add(diagButtons);
        diagSection.Add(_diagnosticInfoArea);
        _layout.Add(diagSection);

        // System info
        var sysInfoSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(0, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var sysTitle = new Label
        {
            Text = "系统信息",
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        sysInfoSection.Add(sysTitle);

        var sysInfoLabel = new Label
        {
            Text = "模块列表、系统信息、Rhino 版本和插件版本可查看诊断报告。",
            Font = SystemFonts.Default(UiTokens.FontSizeSmall),
            TextColor = Colors.Gray,
            Wrap = WrapMode.Word
        };
        sysInfoSection.Add(sysInfoLabel);
        _layout.Add(sysInfoSection);

        // Initial load
        RefreshLogs();
    }

    private void RefreshLogs()
    {
        _viewModel.RefreshRecentLogs();
        _logListBox.Items.Clear();

        foreach (var line in _viewModel.RecentLogLines.Take(200))
        {
            _logListBox.Items.Add(line.Length > 200 ? line.Substring(0, 200) + "..." : line);
        }

        if (_logListBox.Items.Count == 0)
        {
            _logListBox.Items.Add("[无日志条目]");
        }
    }

    private void ExportReport()
    {
        try
        {
            var snapshot = _viewModel.CreateSnapshot();
            var reportText = "=== 诊断报告预览 ===\n\n";
            reportText += $"产品: {snapshot.ProductName}\n";
            reportText += $"版本: {snapshot.PluginVersion}\n";
            reportText += $"平台: {snapshot.OperatingSystem} ({snapshot.ProcessArchitecture})\n";
            reportText += $"运行时: {snapshot.RuntimeVersion}\n";
            reportText += $"模块: {snapshot.Modules.Count} 个已注册\n";
            reportText += $"导出时间: {snapshot.ExportTimeUtc}\n";

            _diagnosticInfoArea.Text = reportText;
        }
        catch (Exception ex)
        {
            _diagnosticInfoArea.Text = $"生成诊断报告失败: {ex.Message}";
        }
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _currentPalette = palette;
        _layout.BackgroundColor = palette.Background;
        _scrollable.BackgroundColor = palette.Background;
        _logListBox.BackgroundColor = palette.Surface;
        _diagnosticInfoArea.BackgroundColor = palette.Surface;
        _diagnosticInfoArea.TextColor = palette.TextPrimary;

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
            else if (child is StackLayout sl)
            {
                foreach (var item in sl.Items)
                {
                    if (item.Control is Button b)
                    {
                        b.TextColor = palette.ButtonText;
                    }
                }
            }
        }
    }
}

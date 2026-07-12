using System;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Components;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Pages;

public sealed class SettingsPage : Panel
{
    private readonly SettingsViewModel _viewModel;
    private readonly DynamicLayout _layout;
    private readonly Scrollable _scrollable;
    private readonly RadioButtonList _themeRadioList;
    private readonly CheckBox _autoOpenCheckBox;
    private readonly CheckBox _rememberPageCheckBox;
    private readonly DropDown _logLevelDropDown;
    private ThemePalette? _currentPalette;

    public event EventHandler? OpenConfigDirectoryRequested;

    public SettingsPage(SettingsViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        // Theme radio buttons
        _themeRadioList = new RadioButtonList
        {
            Items =
            {
                "跟随系统",
                "明亮",
                "暗黑"
            },
            Orientation = Orientation.Vertical,
            SelectedKey = MapThemeModeToIndex(_viewModel.SelectedTheme).ToString()
        };
        _themeRadioList.SelectedKeyChanged += (_, _) =>
        {
            var index = int.Parse(_themeRadioList.SelectedKey);
            _viewModel.SelectedTheme = index switch
            {
                0 => "System",
                1 => "Light",
                2 => "Dark",
                _ => "System"
            };
        };

        // Auto open checkbox
        _autoOpenCheckBox = new CheckBox
        {
            Text = "Rhino 启动后自动打开主面板",
            Checked = _viewModel.AutoOpenPanel,
            ThreeState = false
        };
        _autoOpenCheckBox.CheckedChanged += (_, _) =>
        {
            _viewModel.AutoOpenPanel = _autoOpenCheckBox.Checked ?? false;
        };

        // Remember page checkbox
        _rememberPageCheckBox = new CheckBox
        {
            Text = "记住上次打开的页面",
            Checked = _viewModel.RememberLastPage,
            ThreeState = false
        };
        _rememberPageCheckBox.CheckedChanged += (_, _) =>
        {
            _viewModel.RememberLastPage = _rememberPageCheckBox.Checked ?? false;
        };

        // Log level dropdown
        _logLevelDropDown = new DropDown
        {
            Items = { "Debug", "Information", "Warning", "Error" },
            SelectedValue = _viewModel.SelectedLogLevel
        };
            _logLevelDropDown.SelectedValueChanged += (_, _) =>
            {
                _viewModel.SelectedLogLevel = _logLevelDropDown.SelectedValue?.ToString() ?? "Information";
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
            Text = "设置",
            Font = SystemFonts.Bold(UiTokens.FontSizeLargeTitle),
            TextColor = Colors.Black,
            TextAlignment = TextAlignment.Left
        };
        _layout.Add(titleLabel);

        // Appearance section
        AddSection("外观", new Control[]
        {
            new Label
            {
                Text = "主题模式",
                Font = SystemFonts.Default(UiTokens.FontSizeBody),
                TextColor = Colors.Black,
                VerticalAlignment = VerticalAlignment.Center
            },
            _themeRadioList
        });

        // Startup section
        AddSection("启动行为", new Control[]
        {
            _autoOpenCheckBox,
            _rememberPageCheckBox
        });

        // Log section
        var logLevelRow = new DynamicLayout
        {
            Spacing = new Size(UiTokens.SpacingMedium, 0),
            Padding = new Padding(0)
        };
        logLevelRow.AddRow(new Label { Text = "日志级别", VerticalAlignment = VerticalAlignment.Center }, _logLevelDropDown);

        AddSection("日志", new Control[]
        {
            logLevelRow
        });

        // Interface section
        AddSection("界面", new Control[]
        {
            new Label
            {
                Text = "当前语言: 简体中文 (zh-CN)",
                Font = SystemFonts.Default(UiTokens.FontSizeBody),
                TextColor = Colors.Gray
            }
        });

        // Action buttons
        var actionSection = new DynamicLayout
        {
            Padding = new Padding(UiTokens.SpacingMedium),
            Spacing = new Size(UiTokens.SpacingMedium, UiTokens.SpacingSmall),
            BackgroundColor = Colors.White
        };

        var actionTitle = new Label
        {
            Text = "操作",
            Font = SystemFonts.Bold(UiTokens.FontSizeHeading),
            TextColor = Colors.Black
        };
        actionSection.Add(actionTitle);

        var saveButton = new Button
        {
            Text = "保存设置",
            Size = new Size(120, 32)
        };
        saveButton.Click += (_, _) =>
        {
            _viewModel.Save();
            MessageBox.Show("设置已保存。", "保存成功", MessageBoxType.Information);
        };

        var resetButton = new Button
        {
            Text = "恢复默认",
            Size = new Size(120, 32)
        };
        resetButton.Click += (_, _) =>
        {
            var result = MessageBox.Show("确定要恢复默认设置吗？", "确认恢复", MessageBoxType.Question);
            if (result == DialogResult.Yes)
            {
                _viewModel.ResetToDefaults();
                _autoOpenCheckBox.Checked = false;
                _rememberPageCheckBox.Checked = true;
                _themeRadioList.SelectedKey = "0";
                _logLevelDropDown.SelectedValue = "Information";
                MessageBox.Show("设置已恢复为默认值。", "恢复成功", MessageBoxType.Information);
            }
        };

        var openConfigButton = new Button
        {
            Text = "打开配置目录",
            Size = new Size(120, 32)
        };
        openConfigButton.Click += (_, _) => OpenConfigDirectoryRequested?.Invoke(this, EventArgs.Empty);

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = UiTokens.SpacingMedium,
            Items = { saveButton, resetButton, openConfigButton }
        };
        actionSection.Add(buttonRow);
        _layout.Add(actionSection);
    }

    private void AddSection(string title, Control[] controls)
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

        foreach (var control in controls)
        {
            section.Add(control);
        }

        _layout.Add(section);
    }

    private static int MapThemeModeToIndex(string mode)
    {
        return mode?.ToLowerInvariant() switch
        {
            "system" => 0,
            "light" => 1,
            "dark" => 2,
            _ => 0
        };
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
            else if (child is CheckBox cb)
            {
                cb.TextColor = palette.TextPrimary;
            }
            else if (child is RadioButtonList rbl)
            {
                rbl.TextColor = palette.TextPrimary;
            }
            else if (child is DropDown dd)
            {
                dd.TextColor = palette.TextPrimary;
            }
            else if (child is StackLayout sl)
            {
                foreach (var item in sl.Items)
                {
                    if (item.Control is Label l)
                    {
                        l.TextColor = palette.TextPrimary;
                    }
                }
            }
        }
    }
}

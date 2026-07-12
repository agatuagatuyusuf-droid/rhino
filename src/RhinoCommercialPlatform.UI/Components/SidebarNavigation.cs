using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using NavigationItem = RhinoCommercialPlatform.UI.Shell.NavigationItem;
using RhinoCommercialPlatform.UI.Shell;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Components;

public sealed class SidebarNavigation : Panel
{
    private readonly StackLayout _layout;
    private readonly List<NavButton> _buttons = new List<NavButton>();
    private NavigationPageId _selectedPage;
    private ThemePalette? _currentPalette;

    public event EventHandler<NavigationPageId>? NavigationRequested;

    public NavigationPageId SelectedPage
    {
        get => _selectedPage;
        set
        {
            _selectedPage = value;
            UpdateSelection();
        }
    }

    public SidebarNavigation()
    {
        _layout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 1,
            Padding = new Padding(UiTokens.SpacingSmall),
            Width = UiTokens.SidebarWidth
        };

        var items = new List<NavigationItem>
        {
            new NavigationItem(NavigationPageId.Dashboard, "首页", "dashboard"),
            new NavigationItem(NavigationPageId.Modules, "功能模块", "modules"),
            new NavigationItem(NavigationPageId.RuntimeStatus, "运行状态", "status"),
            new NavigationItem(NavigationPageId.Diagnostics, "诊断中心", "diagnostics"),
            new NavigationItem(NavigationPageId.Settings, "设置", "settings"),
            new NavigationItem(NavigationPageId.About, "关于", "about")
        };

        foreach (var item in items)
        {
            var button = new NavButton(item);
            button.NavClick += (_, pageId) =>
            {
                SelectedPage = pageId;
                NavigationRequested?.Invoke(this, pageId);
            };
            _buttons.Add(button);
            _layout.Items.Add(button);
        }

        _layout.Items.Add(new StackLayoutItem(null, true)); // Spacer

        Content = _layout;
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _currentPalette = palette;
        _layout.BackgroundColor = palette.SidebarBackground;

        foreach (var button in _buttons)
        {
            button.ApplyTheme(palette, button.PageId == _selectedPage);
        }
    }

    private void UpdateSelection()
    {
        if (_currentPalette == null)
            return;

        foreach (var button in _buttons)
        {
            button.ApplyTheme(_currentPalette, button.PageId == _selectedPage);
        }
    }

    private sealed class NavButton : Panel
    {
        private readonly Label _label;
        private readonly DynamicLayout _layout;

        public NavigationPageId PageId { get; }
        public event EventHandler<NavigationPageId>? NavClick;

        public NavButton(NavigationItem item)
        {
            PageId = item.PageId;

            _label = new Label
            {
                Text = item.Label,
                Font = SystemFonts.Default(UiTokens.FontSizeBody),
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Wrap = WrapMode.None
            };

            _layout = new DynamicLayout
            {
                Padding = new Padding(UiTokens.SpacingMedium, UiTokens.SpacingSmall),
                Height = 36
            };
            _layout.Add(_label);

            Content = _layout;

            MouseDown += (_, _) =>
            {
                NavClick?.Invoke(this, PageId);
            };
        }

        public void ApplyTheme(ThemePalette palette, bool isSelected)
        {
            if (isSelected)
            {
                _layout.BackgroundColor = palette.NavItemSelected;
            }
            _label.TextColor = isSelected ? palette.NavItemSelectedText : palette.NavItemText;
        }
    }
}

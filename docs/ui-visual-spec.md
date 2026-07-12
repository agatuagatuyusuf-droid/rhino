# UI Visual Specification

## Layout

```
┌─────────────────────────────────────────────────┐
│  AppHeader (44px)                               │
│  ┌──────────────┬──────────────────────────────┐ │
│  │  RhinoCommercialPlatform  │  [🌓] [⚙]       │ │
│  └──────────────┴──────────────────────────────┘ │
├────────┬────────────────────────────────────────┤
│Sidebar │  Content Area                          │
│(180px) │  (scrollable)                          │
│        │                                        │
│ 首页   │  ┌─ Section ──────────────────────┐    │
│ 功能模块│  │  Title                         │    │
│ 运行状态│  │  label  value                  │    │
│ 诊断中心│  │  label  value                  │    │
│ 设置   │  └────────────────────────────────┘    │
│ 关于   │  ┌─ Section ──────────────────────┐    │
│        │  │  ...                            │    │
│        │  └────────────────────────────────┘    │
├────────┴────────────────────────────────────────┤
│  StatusFooter (28px)                            │
│  Status  │  Platform  │  Version                │
└─────────────────────────────────────────────────┘
```

Panel dimensions: 460×600 (default), 360×300 (minimum), resizable.

## Color Palette

### Light Mode

| Token                   | Hex       | Usage                          |
|-------------------------|-----------|--------------------------------|
| Background              | `#F5F5F7` | Page background                |
| SidebarBackground       | `#ECECEE` | Sidebar background             |
| Surface                 | `#FFFFFF` | Cards, sections                |
| SurfaceElevated         | `#FFFFFF` | Elevated surfaces              |
| Border                  | `#D2D2D7` | Dividers, borders              |
| HeaderBackground        | `#E8E8EA` | Header bar                     |
| StatusBarBackground     | `#ECECEE` | Status footer                  |
| TextPrimary             | `#1C1C1E` | Primary text                   |
| TextSecondary           | `#636367` | Secondary text, labels         |
| TextMuted               | `#8E8E93` | Muted text                     |
| Accent                  | `#007AFF` | Accent/links                   |
| AccentText              | `#FFFFFF` | Text on accent                 |
| Hover                   | `#E8E8EA` | Hover state                    |
| Selected                | `#D0D0D5` | Selected state                 |
| Pressed                 | `#BCBCC2` | Pressed state                  |
| Success                 | `#34C759` | Success indicators             |
| Warning                 | `#FF9F0A` | Warning indicators             |
| Error                   | `#FF3B30` | Error indicators               |
| Info                    | `#007AFF` | Info indicators                |
| CardBackground          | `#FFFFFF` | Card surfaces                  |
| CardBorder              | `#E5E5EA` | Card borders                   |
| NavItemHover            | `#D8D8DA` | Nav item hover                 |
| NavItemSelected         | `#C0C0C5` | Nav item selected              |
| NavItemText             | `#3A3A3C` | Nav item text                  |
| NavItemSelectedText     | `#1C1C1E` | Nav item selected text         |
| ButtonBackground        | `#E8E8EA` | Default button background      |
| ButtonText              | `#1C1C1E` | Button text                    |
| ButtonHover             | `#D8D8DA` | Button hover                   |

### Dark Mode

| Token                   | Hex       | Usage                          |
|-------------------------|-----------|--------------------------------|
| Background              | `#1C1C1E` | Page background                |
| SidebarBackground       | `#252527` | Sidebar background             |
| Surface                 | `#2C2C2E` | Cards, sections                |
| SurfaceElevated         | `#363638` | Elevated surfaces              |
| Border                  | `#444446` | Dividers, borders              |
| HeaderBackground        | `#252527` | Header bar                     |
| StatusBarBackground     | `#252527` | Status footer                  |
| TextPrimary             | `#F5F5F7` | Primary text                   |
| TextSecondary           | `#A8A8AD` | Secondary text, labels         |
| TextMuted               | `#8E8E93` | Muted text                     |
| Accent                  | `#0A84FF` | Accent/links                   |
| AccentText              | `#FFFFFF` | Text on accent                 |
| Hover                   | `#3A3A3C` | Hover state                    |
| Selected                | `#48484A` | Selected state                 |
| Pressed                 | `#555557` | Pressed state                  |
| Success                 | `#30D158` | Success indicators             |
| Warning                 | `#FF9F0A` | Warning indicators             |
| Error                   | `#FF453A` | Error indicators               |
| Info                    | `#0A84FF` | Info indicators                |
| CardBackground          | `#2C2C2E` | Card surfaces                  |
| CardBorder              | `#444446` | Card borders                   |
| NavItemHover            | `#3A3A3C` | Nav item hover                 |
| NavItemSelected         | `#48484A` | Nav item selected              |
| NavItemText             | `#A8A8AD` | Nav item text                  |
| NavItemSelectedText     | `#F5F5F7` | Nav item selected text         |
| ButtonBackground        | `#3A3A3C` | Default button background      |
| ButtonText              | `#F5F5F7` | Button text                    |
| ButtonHover             | `#48484A` | Button hover                   |

## Typography

| Style        | Size | Weight | Usage                            |
|--------------|------|--------|----------------------------------|
| Large Title  | 18pt | Bold   | Page titles                      |
| Title        | 15pt | Bold   | Section headings                 |
| Heading      | 13pt | Bold   | Card/section titles              |
| Body         | 13pt | Normal | Content text, values             |
| Small        | 11pt | Normal | Labels, metadata, status bar     |

Font family: System font (SF Pro on macOS, Segoe UI on Windows) via `SystemFonts.Default()` and `SystemFonts.Bold()`.

## Spacing and Sizing

| Token      | Value | Usage                        |
|------------|-------|------------------------------|
| SpacingXS  | 4px   | Tight spacing                |
| SpacingSM  | 8px   | Default spacing              |
| SpacingMD  | 16px  | Section padding              |
| SpacingLG  | 24px  | Page padding                 |
| SpacingXL  | 32px  | Large separators             |

### Components

| Component            | Dimensions          | Notes                         |
|----------------------|---------------------|-------------------------------|
| Header               | Height: 44px        | Full-width, title + buttons   |
| Sidebar              | Width: 180px        | Vertical nav items            |
| Sidebar nav item     | Height: 36px        | Padding: 8px 4px              |
| Status bar           | Height: 28px        | Full-width, 3-column layout   |
| Info card            | Auto                | Padding: 16px                 |
| Buttons              | 32×32 (icon), 100×36 (text), 120×32 (action) |        |
| Section label width  | 120px               | Label column in info rows     |
| Panel                | 460×600 (default)   | Min: 360×300                  |

## Corner Radii

| Token    | Value | Usage                |
|----------|-------|----------------------|
| Small    | 4px   | Default rounding     |
| Medium   | 8px   | Cards, elevated      |
| Large    | 12px  | Dialogs              |

## Component Descriptions

### AppHeader
- Displays "RhinoCommercialPlatform" title in bold 18pt
- Right-aligned: theme toggle button (🌓) and settings button (⚙)
- Background: `HeaderBackground` color

### SidebarNavigation
- Fixed 180px width, vertical stack
- Items: 首页, 功能模块, 运行状态, 诊断中心, 设置, 关于
- Selected item has distinct background (`NavItemSelected`) and text color
- Clicking an item emits navigation event

### StatusFooter
- 28px height, three-column layout
- Left: status text; Center: platform info; Right: version
- All text in muted color (`TextMuted`)

### InfoCard
- Two-line card with title (small, secondary) and value (body, primary)
- Used for displaying key=value information
- Background: `CardBackground`

### EmptyStateView
- Centered message with optional description
- Used when a page has no data to display

### ErrorStateView
- Centered error message with detail in red tones
- Used for error display states

### Pages (Dashboard, Modules, RuntimeStatus, Diagnostics, Settings, About)
- Each page is a `Panel` inside a `Scrollable`
- Pages use `DynamicLayout` with sections
- Sections have a bold heading and rows of label-value pairs
- Dashboard has quick-entry buttons for navigation
- Settings has interactive controls: radio buttons (theme), checkboxes, dropdown
- Diagnostics has a log list box, diagnostic text area, and action buttons

namespace RhinoCommercialPlatform.UI.Shell;

public sealed class NavigationItem
{
    public NavigationPageId PageId { get; }
    public string Label { get; }
    public string IconKey { get; }
    public bool IsEnabled { get; set; } = true;

    public NavigationItem(NavigationPageId pageId, string label, string iconKey)
    {
        PageId = pageId;
        Label = label ?? throw new ArgumentNullException(nameof(label));
        IconKey = iconKey ?? throw new ArgumentNullException(nameof(iconKey));
    }
}

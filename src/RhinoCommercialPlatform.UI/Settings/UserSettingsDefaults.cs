namespace RhinoCommercialPlatform.UI.Settings;

public static class UserSettingsDefaults
{
    public const int SchemaVersion = 1;
    public const string ThemeMode = "System";
    public const bool AutoOpenPanel = false;
    public const bool RememberLastPage = true;
    public const string LastPage = "Dashboard";
    public const string LogLevel = "Information";
    public const string Language = "zh-CN";

    public static readonly string[] ValidThemeModes = { "System", "Light", "Dark" };
    public static readonly string[] ValidLogLevels = { "Debug", "Information", "Warning", "Error" };
    public static readonly string[] ValidPages = { "Dashboard", "Modules", "RuntimeStatus", "Diagnostics", "Settings", "About" };
}

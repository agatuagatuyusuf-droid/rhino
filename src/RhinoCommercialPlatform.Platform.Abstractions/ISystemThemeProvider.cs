namespace RhinoCommercialPlatform.Platform.Abstractions;

public enum SystemTheme
{
    Light = 0,
    Dark = 1,
    Unknown = 2
}

public interface ISystemThemeProvider
{
    SystemTheme GetCurrentSystemTheme();
}

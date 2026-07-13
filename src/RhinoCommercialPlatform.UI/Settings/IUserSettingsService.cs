namespace RhinoCommercialPlatform.UI.Settings;

public interface IUserSettingsService
{
    UserSettings Load();
    void Save(UserSettings settings);
    UserSettings ResetToDefaults();
}

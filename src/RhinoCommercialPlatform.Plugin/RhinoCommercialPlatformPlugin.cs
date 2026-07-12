using System;
using Rhino;
using Rhino.PlugIns;
using RhinoCommercialPlatform.Plugin.Panels;

namespace RhinoCommercialPlatform.Plugin;

public sealed class RhinoCommercialPlatformPlugin : PlugIn
{
    public static RhinoCommercialPlatformPlugin? Instance
    {
        get;
        private set;
    }

    internal AppRuntime? Runtime
    {
        get;
        private set;
    }

    private bool _autoOpenHandled;

    public RhinoCommercialPlatformPlugin()
    {
        Instance = this;
    }

    protected override LoadReturnCode OnLoad(
        ref string errorMessage)
    {
        if (Runtime != null)
        {
            errorMessage = "Runtime is already initialized.";
            return LoadReturnCode.ErrorShowDialog;
        }

        try
        {
            Runtime = AppRuntime.Start();

            // Register the main panel
            MainPanelRegistration.Register();

            RhinoApp.WriteLine(
                $"RhinoCommercialPlatform " +
                $"{Runtime.Metadata.Version} loaded.");

            // Handle auto-open panel after Rhino is ready
            HandleAutoOpen();

            return LoadReturnCode.Success;
        }
        catch (Exception startupException)
        {
            var details =
                $"Failed to start RhinoCommercialPlatform runtime: " +
                $"{startupException.GetType().Name}: " +
                $"{startupException.Message}";

            if (Runtime != null)
            {
                try
                {
                    Runtime.Dispose();
                }
                catch (Exception cleanupException)
                {
                    details +=
                        $" Cleanup also failed: " +
                        $"{cleanupException.GetType().Name}: " +
                        $"{cleanupException.Message}";

                    RhinoApp.WriteLine(details);
                }
                finally
                {
                    Runtime = null;
                }
            }

            errorMessage = details;
            return LoadReturnCode.ErrorShowDialog;
        }
    }

    protected override void OnShutdown()
    {
        try
        {
            var panelService = Panels.MainPanelService.GetInstance(Runtime!);
            panelService.Dispose();
        }
        catch
        {
            // Best effort cleanup
        }

        try
        {
            Runtime?.Dispose();
        }
        catch (Exception shutdownException)
        {
            RhinoApp.WriteLine(
                $"RhinoCommercialPlatform shutdown failed: " +
                $"{shutdownException.GetType().Name}: " +
                $"{shutdownException.Message}");
        }
        finally
        {
            Runtime = null;
            Instance = null;
        }
    }

    private void HandleAutoOpen()
    {
        if (_autoOpenHandled)
            return;

        try
        {
            if (Runtime == null)
                return;

            var settings = Runtime.UserSettingsService.Load();
            if (!settings.AutoOpenPanel)
            {
                Runtime.Logger.Debug("Auto-open panel is disabled.");
                return;
            }

            // Use Idle event to open panel after Rhino is ready
            RhinoApp.Idle += OnRhinoIdleAutoOpen;
        }
        catch (Exception ex)
        {
            Runtime?.Logger.Error($"Failed to schedule auto-open: {ex.Message}");
        }
    }

    private void OnRhinoIdleAutoOpen(object? sender, EventArgs e)
    {
        try
        {
            RhinoApp.Idle -= OnRhinoIdleAutoOpen;

            if (_autoOpenHandled)
                return;

            _autoOpenHandled = true;

            MainPanelRegistration.OpenPanel();
            Runtime?.Logger.Information("Panel auto-opened on startup.");
        }
        catch (Exception ex)
        {
            Runtime?.Logger.Error($"Auto-open failed: {ex.Message}");
        }
    }
}

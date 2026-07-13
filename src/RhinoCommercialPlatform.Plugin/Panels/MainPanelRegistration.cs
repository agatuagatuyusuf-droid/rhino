using System;
using Rhino;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Plugin.Panels;

public enum RegistrationState
{
    NotAttempted,
    Succeeded,
    Failed
}

public static class MainPanelRegistration
{
    private static readonly object _lock = new();
    private static RegistrationState _state = RegistrationState.NotAttempted;
    private static string? _lastError;
    private static IRhinoPanelGateway? _gateway;

    public static readonly Guid PanelId = new("7B3E4F2A-1D8C-4E5F-9A6B-3C2D1E0F8A7B");
    public const string PanelName = "RhinoCommercialPlatform";

    public static RegistrationState State => _state;
    public static string? LastError => _lastError;

    internal static void SetGateway(IRhinoPanelGateway? gateway)
    {
        lock (_lock) { _gateway = gateway; }
    }

    internal static void ResetState()
    {
        lock (_lock)
        {
            _state = RegistrationState.NotAttempted;
            _lastError = null;
        }
    }

    private static IRhinoPanelGateway GetGateway()
    {
        var g = _gateway;
        if (g == null)
        {
            g = new RhinoPanelGateway();
            _gateway = g;
        }
        return g;
    }

    public static void Register()
    {
        lock (_lock)
        {
            if (_state == RegistrationState.Succeeded)
            {
                RhinoApp.WriteLine($"Panel '{PanelName}' already registered.");
                return;
            }

            var plugin = RhinoCommercialPlatformPlugin.Instance;
            if (plugin == null)
            {
                _lastError = $"Cannot register panel '{PanelName}': plugin instance is null.";
                _state = RegistrationState.Failed;
                RhinoApp.WriteLine(_lastError);
                throw new InvalidOperationException(_lastError);
            }

            var gateway = GetGateway();
            try
            {
                var success = gateway.RegisterPanel(plugin, typeof(RhinoMainPanelHost), PanelName, null);
                if (!success)
                {
                    _lastError = $"Failed to register panel '{PanelName}' — API returned failure.";
                    _state = RegistrationState.Failed;
                    RhinoApp.WriteLine(_lastError);
                    throw new InvalidOperationException(_lastError);
                }

                _state = RegistrationState.Succeeded;
                _lastError = null;
                RhinoApp.WriteLine($"Panel '{PanelName}' registered successfully.");
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _lastError = $"Failed to register panel '{PanelName}': {ex.Message}";
                _state = RegistrationState.Failed;
                RhinoApp.WriteLine(_lastError);
                throw new InvalidOperationException(_lastError, ex);
            }
        }
    }

    public static bool EnsureRegistered()
    {
        lock (_lock)
        {
            if (_state == RegistrationState.Succeeded)
                return true;
        }

        try
        {
            Register();
            return _state == RegistrationState.Succeeded;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"EnsureRegistered failed: {ex.Message}");
            return false;
        }
    }

    public static bool OpenPanel()
    {
        try
        {
            if (!EnsureRegistered())
            {
                RhinoApp.WriteLine($"Cannot open panel '{PanelName}': not registered.");
                return false;
            }

            var gateway = GetGateway();
            var opened = gateway.OpenPanel(typeof(RhinoMainPanelHost), true);
            if (!opened)
            {
                RhinoApp.WriteLine($"Failed to open panel '{PanelName}': OpenPanel returned false.");
                return false;
            }

            var visible = gateway.IsPanelVisible(typeof(RhinoMainPanelHost));
            var panelInstance = gateway.GetPanel(PanelId);
            var instanceOk = panelInstance != null && panelInstance is RhinoMainPanelHost;

            if (visible && instanceOk)
            {
                RhinoApp.WriteLine($"Panel '{PanelName}' opened, visible, instance OK.");
                return true;
            }

            RhinoApp.WriteLine($"Panel '{PanelName}': open call returned true but visible={visible}, instance={instanceOk}");
            return false;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to open panel '{PanelName}': {ex.Message}");
            return false;
        }
    }

    public static void ClosePanel()
    {
        try
        {
            var gateway = GetGateway();
            gateway.ClosePanel(PanelId);

            try
            {
                if (gateway.IsPanelVisible(typeof(RhinoMainPanelHost)))
                {
                    RhinoApp.WriteLine($"Panel '{PanelName}' close called but panel still reported visible.");
                }
            }
            catch { }

            RhinoApp.WriteLine($"Panel '{PanelName}' closed.");
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to close panel '{PanelName}': {ex.Message}");
        }
    }

    public static bool IsPanelVisible()
    {
        try
        {
            return GetGateway().IsPanelVisible(typeof(RhinoMainPanelHost));
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to check panel visibility '{PanelName}': {ex.Message}");
            return false;
        }
    }

    public static bool IsRegistered()
    {
        return _state == RegistrationState.Succeeded;
    }
}

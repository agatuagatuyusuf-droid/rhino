using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Commands;
using RhinoCommercialPlatform.Plugin.Panels;
using RhinoCommercialPlatform.UI.Shell;

namespace RhinoCommercialPlatform.Plugin.Commands;

public sealed class VerifyPanelCommand : Command
{
    public override string EnglishName => "RCP_VerifyPanel";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        var failures = new List<string>();
        var allPassed = true;

        void Pass(string name)
        {
            RhinoApp.WriteLine($"RCP_VERIFY:{name}:PASS");
        }

        void Fail(string name, string reason)
        {
            RhinoApp.WriteLine($"RCP_VERIFY:{name}:FAIL:{reason}");
            allPassed = false;
            failures.Add(name);
        }

        var plugin = RhinoCommercialPlatformPlugin.Instance;
        if (plugin != null)
            Pass("PluginInstance");
        else
            Fail("PluginInstance", "plugin is null");

        if (plugin == null)
        {
            RhinoApp.WriteLine("RCP_PANEL_VERIFY_FAIL");
            return Result.Failure;
        }

        var runtime = plugin.Runtime;
        if (runtime != null)
            Pass("RuntimeExists");
        else
            Fail("RuntimeExists", "runtime is null");

        if (runtime == null)
        {
            RhinoApp.WriteLine("RCP_PANEL_VERIFY_FAIL");
            return Result.Failure;
        }

        var logger = runtime.Logger;

        try
        {
            if (MainPanelRegistration.EnsureRegistered())
                Pass("EnsureRegistered");
            else
                Fail("EnsureRegistered", "EnsureRegistered returned false");
        }
        catch (Exception ex)
        {
            Fail("EnsureRegistered", ex.Message);
        }

        try
        {
            if (MainPanelRegistration.OpenPanel())
                Pass("OpenPanel");
            else
                Fail("OpenPanel", "OpenPanel returned false");
        }
        catch (Exception ex)
        {
            Fail("OpenPanel", ex.Message);
        }

        try
        {
            if (MainPanelRegistration.IsPanelVisible())
                Pass("IsPanelVisible");
            else
                Fail("IsPanelVisible", "panel is not visible");
        }
        catch (Exception ex)
        {
            Fail("IsPanelVisible", ex.Message);
        }

        object? panelInstance = null;
        try
        {
            var gateway = new RhinoPanelGateway();
            panelInstance = gateway.GetPanel(MainPanelRegistration.PanelId);
            if (panelInstance is RhinoMainPanelHost)
                Pass("GetPanel");
            else if (panelInstance == null)
                Fail("GetPanel", "GetPanel returned null");
            else
                Fail("GetPanel", $"unexpected type: {panelInstance.GetType().FullName}");
        }
        catch (Exception ex)
        {
            Fail("GetPanel", ex.Message);
        }

        try
        {
            var panelId = MainPanelRegistration.PanelId;
            var hostGuidAttr = typeof(RhinoMainPanelHost).GetCustomAttribute<GuidAttribute>();
            if (hostGuidAttr != null && Guid.TryParse(hostGuidAttr.Value, out var attrGuid) && panelId == attrGuid)
                Pass("PanelGuidMatch");
            else
                Fail("PanelGuidMatch", hostGuidAttr == null ? "RhinoMainPanelHost missing GuidAttribute" : "GUID mismatch");
        }
        catch (Exception ex)
        {
            Fail("PanelGuidMatch", ex.Message);
        }

        try
        {
            var panelService = MainPanelService.GetInstance(runtime);
            var view = panelService.GetOrCreatePanelView();
            if (view != null)
            {
                Pass("MainPanelViewCreated");
                logger.Information("MainPanelView type: " + typeof(MainPanelView).FullName);
            }
            else
            {
                Fail("MainPanelViewCreated", "MainPanelView is null");
            }
        }
        catch (Exception ex)
        {
            Fail("MainPanelViewCreated", ex.Message);
        }

        var settingsSaved = false;
        try
        {
            var settingsService = runtime.UserSettingsService;
            var settings = settingsService.Load();
            settingsService.Save(settings);
            settingsSaved = true;
            Pass("SettingsFirstSave");
        }
        catch (Exception ex)
        {
            Fail("SettingsFirstSave", ex.Message);
        }

        var settingsReloaded = false;
        try
        {
            var settingsService = runtime.UserSettingsService;
            var reloaded = settingsService.Load();
            settingsReloaded = reloaded != null;
            if (settingsReloaded)
                Pass("SettingsReload");
            else
                Fail("SettingsReload", "Load returned null");
        }
        catch (Exception ex)
        {
            Fail("SettingsReload", ex.Message);
        }

        try
        {
            var tmpFiles = Directory.GetFiles(runtime.Paths.ConfigDirectory, "*.tmp");
            if (tmpFiles.Length == 0)
                Pass("NoTempFiles");
            else
                Fail("NoTempFiles", $"{tmpFiles.Length} temp file(s) remain in config directory");
        }
        catch (Exception ex)
        {
            Fail("NoTempFiles", ex.Message);
        }

        try
        {
            if (Directory.Exists(runtime.Paths.LogsDirectory))
                Pass("LogDirectoryExists");
            else
                Fail("LogDirectoryExists", "log directory does not exist");
        }
        catch (Exception ex)
        {
            Fail("LogDirectoryExists", ex.Message);
        }

        var pluginVersion = "unknown";
        try
        {
            pluginVersion = runtime.Metadata.Version;
            if (!string.IsNullOrWhiteSpace(pluginVersion))
                Pass("PluginVersion");
            else
                Fail("PluginVersion", "plugin version is empty");
        }
        catch (Exception ex)
        {
            Fail("PluginVersion", ex.Message);
        }

        var rhinoVersion = "unknown";
        try
        {
            rhinoVersion = RhinoApp.ExeVersion.ToString();
            if (!string.IsNullOrWhiteSpace(rhinoVersion))
                Pass("RhinoVersion");
            else
                Fail("RhinoVersion", "rhino version is empty");
        }
        catch (Exception ex)
        {
            Fail("RhinoVersion", ex.Message);
        }

        if (allPassed)
        {
            RhinoApp.WriteLine("RCP_PANEL_VERIFY_PASS");
            logger.Information("RCP_VerifyPanel: all checks passed");
        }
        else
        {
            RhinoApp.WriteLine("RCP_PANEL_VERIFY_FAIL");
            logger.Error("RCP_VerifyPanel: " + failures.Count + " check(s) failed: " + string.Join(", ", failures));
        }

        try
        {
            var validationDir = Path.Combine(runtime.Paths.DataDirectory, "validation");
            Directory.CreateDirectory(validationDir);
            var jsonPath = Path.Combine(validationDir, "rhino-panel-verification.json");
            var platform = runtime.Platform;
            var panelId = MainPanelRegistration.PanelId;
            var visible = false;
            try { visible = MainPanelRegistration.IsPanelVisible(); } catch { }
            var panelTypeName = panelInstance?.GetType().FullName ?? "null";

            var json = "{" +
                "\"status\":\"" + (allPassed ? "pass" : "fail") + "\"," +
                "\"sourceCommit\":\"unknown\"," +
                "\"testedCommit\":\"unknown\"," +
                "\"pluginVersion\":\"" + pluginVersion + "\"," +
                "\"rhinoVersion\":\"" + rhinoVersion + "\"," +
                "\"platform\":\"" + platform.OperatingSystem + "\"," +
                "\"architecture\":\"" + platform.ProcessArchitecture + "\"," +
                "\"panelId\":\"" + panelId.ToString() + "\"," +
                "\"panelVisible\":" + visible.ToString().ToLower() + "," +
                "\"panelInstanceType\":\"" + panelTypeName + "\"," +
                "\"settingsFirstSave\":" + settingsSaved.ToString().ToLower() + "," +
                "\"settingsReload\":" + settingsReloaded.ToString().ToLower() + "," +
                "\"logDirectoryExists\":" + Directory.Exists(runtime.Paths.LogsDirectory).ToString().ToLower() + "," +
                "\"testedAtUtc\":\"" + DateTime.UtcNow.ToString("O") + "\"," +
                "\"failures\":[" + string.Join(",", failures.Select(f => "\"" + f + "\"")) + "]" +
                "}";

            File.WriteAllText(jsonPath, json);
            logger.Information("RCP_VerifyPanel: validation result written to " + jsonPath);
        }
        catch (Exception ex)
        {
            logger.Error("RCP_VerifyPanel: failed to write validation result: " + ex.Message);
        }

        return allPassed ? Result.Success : Result.Failure;
    }
}

using System;
using System.Runtime.InteropServices;
using Rhino;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Plugin;

/// <summary>
/// Detects the current system/Rhino theme.
/// Runs inside Rhino context, so it has access to RhinoCommon and platform APIs.
/// On macOS: reads AppleInterfaceStyle via /usr/bin/defaults command.
/// On Windows: reads registry theme setting.
/// Both paths include timeout, logging, and absolute binary paths.
/// </summary>
public sealed class PluginSystemThemeProvider : ISystemThemeProvider
{
    private const int MacProcessTimeoutMs = 3000;
    private const int WindowsProcessTimeoutMs = 2000;

    /// <summary>
    /// Returns the current system theme, with fallback to Light on failure.
    /// </summary>
    public SystemTheme GetCurrentSystemTheme()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacSystemTheme();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsSystemTheme();
            }

            RhinoApp.WriteLine("Theme detection: unsupported OS platform, returning Light.");
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Theme detection failed: {ex.Message}");
        }

        return SystemTheme.Light;
    }

    private static SystemTheme GetMacSystemTheme()
    {
        try
        {
            using var proc = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    // Use absolute path to avoid PATH resolution issues
                    FileName = "/usr/bin/defaults",
                    Arguments = "read -g AppleInterfaceStyle",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            // Read output with timeout
            var output = string.Empty;
            if (proc.WaitForExit(MacProcessTimeoutMs))
            {
                output = proc.StandardOutput.ReadToEnd()?.Trim() ?? string.Empty;
            }
            else
            {
                RhinoApp.WriteLine("Theme detection (macOS): 'defaults read' timed out after {MacProcessTimeoutMs}ms.");
                try { proc.Kill(entireProcessTree: true); } catch { /* best-effort cleanup */ }
                return SystemTheme.Light;
            }

            if (string.Equals(output, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                RhinoApp.WriteLine("Theme detection (macOS): system theme = Dark.");
                return SystemTheme.Dark;
            }

            RhinoApp.WriteLine("Theme detection (macOS): system theme = Light.");
            return SystemTheme.Light;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Theme detection (macOS) failed: {ex.Message}");
            return SystemTheme.Light;
        }
    }

    private static SystemTheme GetWindowsSystemTheme()
    {
        try
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string valueName = "AppsUseLightTheme";

            if (TryReadRegistryDword(Microsoft.Win32.RegistryHive.CurrentUser, keyPath, valueName, out var value))
            {
                // 0 = dark mode, 1 = light mode
                if (value == 0)
                {
                    RhinoApp.WriteLine("Theme detection (Windows): system theme = Dark.");
                    return SystemTheme.Dark;
                }

                RhinoApp.WriteLine("Theme detection (Windows): system theme = Light.");
                return SystemTheme.Light;
            }

            RhinoApp.WriteLine("Theme detection (Windows): could not read registry value, returning Light.");
            return SystemTheme.Light;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Theme detection (Windows) failed: {ex.Message}");
            return SystemTheme.Light;
        }
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int RegGetValueW(
        IntPtr hKey,
        string lpSubKey,
        string lpValueName,
        uint dwFlags,
        out uint pdwType,
        out uint pvData,
        ref uint pcbData);

    private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
    private const uint RRF_RT_DWORD = 0x00000018;

    private static bool TryReadRegistryDword(
        Microsoft.Win32.RegistryHive hive,
        string subKey,
        string valueName,
        out uint value)
    {
        value = 0;
        try
        {
            IntPtr hKey = hive switch
            {
                Microsoft.Win32.RegistryHive.CurrentUser => HKEY_CURRENT_USER,
                _ => throw new NotSupportedException($"Unsupported hive: {hive}")
            };

            uint type = 0;
            uint data = 0;
            uint cbData = 4;

            int result = RegGetValueW(hKey, subKey, valueName, RRF_RT_DWORD, out type, out data, ref cbData);
            if (result == 0) // ERROR_SUCCESS
            {
                value = data;
                return true;
            }
        }
        catch
        {
            // Fall through
        }
        return false;
    }
}

using System;
using System.Runtime.InteropServices;
using Rhino;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Plugin;

/// <summary>
/// Detects the current system/Rhino theme.
/// Runs inside Rhino context, so it has access to RhinoCommon and platform APIs.
/// On macOS: reads AppleInterfaceStyle via defaults command.
/// On Windows: reads registry theme setting.
/// </summary>
public sealed class PluginSystemThemeProvider : ISystemThemeProvider
{
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
                    FileName = "defaults",
                    Arguments = "read -g AppleInterfaceStyle",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd()?.Trim();
            proc.WaitForExit(500);

            if (string.Equals(output, "Dark", StringComparison.OrdinalIgnoreCase))
                return SystemTheme.Dark;

            return SystemTheme.Light;
        }
        catch
        {
            return SystemTheme.Light;
        }
    }

    private static SystemTheme GetWindowsSystemTheme()
    {
        try
        {
            // Use RegGetValue via P/Invoke to avoid adding Microsoft.Win32.Registry dependency
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string valueName = "AppsUseLightTheme";

            if (TryReadRegistryDword(Microsoft.Win32.RegistryHive.CurrentUser, keyPath, valueName, out var value))
            {
                // 0 = dark mode, 1 = light mode
                if (value == 0)
                    return SystemTheme.Dark;
            }

            return SystemTheme.Light;
        }
        catch
        {
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

using System;
using System.Diagnostics;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Platform.Windows;

public sealed class WindowsExternalLauncher : IExternalLauncher
{
    public bool OpenDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be empty.", nameof(directoryPath));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{directoryPath}\"",
                UseShellExecute = true,
                Verb = "open"
            };
            using var process = Process.Start(psi);
            return process != null && !process.HasExited;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool OpenFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "open"
            };
            using var process = Process.Start(psi);
            return process != null && !process.HasExited;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                Verb = "open"
            };
            using var process = Process.Start(psi);
            return process != null && !process.HasExited;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

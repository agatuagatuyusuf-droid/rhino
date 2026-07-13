using System;
using System.Diagnostics;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Platform.Mac;

public sealed class MacExternalLauncher : IExternalLauncher
{
    public bool OpenDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be empty.", nameof(directoryPath));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/usr/bin/open",
                Arguments = $"\"{directoryPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if (process == null)
                return false;
            process.WaitForExit(5000);
            return process.ExitCode == 0;
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
                FileName = "/usr/bin/open",
                Arguments = $"\"{filePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if (process == null)
                return false;
            process.WaitForExit(5000);
            return process.ExitCode == 0;
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
                FileName = "/usr/bin/open",
                Arguments = $"\"{url}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if (process == null)
                return false;
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

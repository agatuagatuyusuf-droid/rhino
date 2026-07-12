using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RhinoCommercialPlatform.UI.Diagnostics;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class DiagnosticsViewModel
{
    private readonly DiagnosticReportService _reportService;
    private readonly string _logDirectory;
    private Func<DiagnosticSnapshot>? _snapshotFactory;

    public string LogDirectory => _logDirectory;
    public ObservableCollection<string> RecentLogLines { get; } = new ObservableCollection<string>();

    public DiagnosticsViewModel(DiagnosticReportService reportService, string logDirectory)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
    }

    public void SetSnapshotFactory(Func<DiagnosticSnapshot> factory)
    {
        _snapshotFactory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public void RefreshRecentLogs()
    {
        RecentLogLines.Clear();

        var logFile = GetRecentLogFilePath();
        if (logFile == null)
        {
            RecentLogLines.Add("[没有找到日志文件]");
            return;
        }

        var lines = RecentLogReader.ReadLastLines(logFile, 200);
        foreach (var line in lines)
        {
            RecentLogLines.Add(line);
        }

        if (RecentLogLines.Count == 0)
        {
            RecentLogLines.Add("[日志文件为空]");
        }
    }

    public string? ExportDiagnosticReport(string outputDirectory)
    {
        if (_snapshotFactory == null)
            return null;

        var snapshot = _snapshotFactory();
        return _reportService.ExportReport(snapshot, outputDirectory);
    }

    public DiagnosticSnapshot CreateSnapshot()
    {
        if (_snapshotFactory == null)
            throw new InvalidOperationException("Snapshot factory not set.");
        return _snapshotFactory();
    }

    private string? GetRecentLogFilePath()
    {
        if (!System.IO.Directory.Exists(_logDirectory))
            return null;

        var logFiles = System.IO.Directory.GetFiles(_logDirectory, "plugin-*.log");
        if (logFiles.Length == 0)
            return null;

        string? mostRecent = null;
        DateTime mostRecentTime = DateTime.MinValue;

        foreach (var file in logFiles)
        {
            try
            {
                var lastWrite = System.IO.File.GetLastWriteTimeUtc(file);
                if (lastWrite > mostRecentTime)
                {
                    mostRecentTime = lastWrite;
                    mostRecent = file;
                }
            }
            catch
            {
                // Skip files we can't access
            }
        }

        return mostRecent;
    }
}

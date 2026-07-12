using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RhinoCommercialPlatform.Infrastructure;

namespace RhinoCommercialPlatform.UI.Diagnostics;

public sealed class DiagnosticReportService
{
    private readonly string _logDirectory;
    private readonly string _configDirectory;

    public DiagnosticReportService(string logDirectory, string configDirectory)
    {
        _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
        _configDirectory = configDirectory ?? throw new ArgumentNullException(nameof(configDirectory));
    }

    public DiagnosticSnapshot CreateSnapshot(
        string productName,
        string pluginVersion,
        string? commitInfo,
        string operatingSystem,
        string processArchitecture,
        string runtimeVersion,
        string rhinoVersion,
        string runMode,
        IReadOnlyList<DiagnosticSnapshot.ModuleInfo> modules)
    {
        var recentLogFile = GetRecentLogFile();
        var recentLines = new List<string>();

        if (recentLogFile != null)
        {
            recentLines = RecentLogReader.ReadLastLines(recentLogFile, 200);
        }

        return new DiagnosticSnapshot
        {
            ProductName = productName,
            PluginVersion = pluginVersion,
            CommitInfo = commitInfo,
            OperatingSystem = operatingSystem,
            ProcessArchitecture = processArchitecture,
            RuntimeVersion = runtimeVersion,
            RhinoVersion = rhinoVersion,
            LogDirectory = _logDirectory,
            ConfigDirectory = _configDirectory,
            Modules = new List<DiagnosticSnapshot.ModuleInfo>(modules),
            RecentLogLines = recentLines,
            ExportTimeUtc = DateTime.UtcNow.ToString("o"),
            RunMode = runMode
        };
    }

    public string GenerateReportText(DiagnosticSnapshot snapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("  RhinoCommercialPlatform 诊断报告");
        sb.AppendLine("  Diagnostic Report");
        sb.AppendLine("========================================");
        sb.AppendLine();

        sb.AppendLine($"产品名称: {SecretRedactor.Redact(snapshot.ProductName ?? "")}");
        sb.AppendLine($"插件版本: {SecretRedactor.Redact(snapshot.PluginVersion ?? "")}");
        if (!string.IsNullOrEmpty(snapshot.CommitInfo))
            sb.AppendLine($"Build Commit: {SecretRedactor.Redact(snapshot.CommitInfo!)}");
        sb.AppendLine($"运行模式: {SecretRedactor.Redact(snapshot.RunMode ?? "")}");
        sb.AppendLine($"操作系统: {SecretRedactor.Redact(snapshot.OperatingSystem ?? "")}");
        sb.AppendLine($"进程架构: {SecretRedactor.Redact(snapshot.ProcessArchitecture ?? "")}");
        sb.AppendLine($"运行时版本: {SecretRedactor.Redact(snapshot.RuntimeVersion ?? "")}");
        sb.AppendLine($"Rhino 版本: {SecretRedactor.Redact(snapshot.RhinoVersion ?? "")}");
        sb.AppendLine();
        sb.AppendLine("--- 路径信息 ---");
        sb.AppendLine($"日志目录: {SecretRedactor.Redact(snapshot.LogDirectory ?? "")}");
        sb.AppendLine($"配置目录: {SecretRedactor.Redact(snapshot.ConfigDirectory ?? "")}");
        sb.AppendLine();

        sb.AppendLine("--- 模块列表 ---");
        foreach (var module in snapshot.Modules)
        {
            sb.AppendLine($"  [{module.Status}] {module.Id} ({module.DisplayName}) v{module.Version}");
        }
        sb.AppendLine();

        sb.AppendLine("--- 最近日志 (最多200行) ---");
        foreach (var line in snapshot.RecentLogLines)
        {
            sb.AppendLine(SecretRedactor.Redact(line));
        }
        sb.AppendLine();

        sb.AppendLine($"--- 导出时间: {snapshot.ExportTimeUtc} ---");
        sb.AppendLine("========================================");

        return sb.ToString();
    }

    public string ExportReport(DiagnosticSnapshot snapshot, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var fileName = $"diagnostic-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";
        var filePath = Path.Combine(outputDirectory, fileName);

        var reportText = GenerateReportText(snapshot);
        File.WriteAllText(filePath, reportText, Encoding.UTF8);

        return filePath;
    }

    private string? GetRecentLogFile()
    {
        if (!Directory.Exists(_logDirectory))
            return null;

        var logFiles = Directory.GetFiles(_logDirectory, "plugin-*.log");
        if (logFiles.Length == 0)
            return null;

        // Get the most recent log file
        string? mostRecent = null;
        DateTime mostRecentTime = DateTime.MinValue;

        foreach (var file in logFiles)
        {
            try
            {
                var lastWrite = File.GetLastWriteTimeUtc(file);
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

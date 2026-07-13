using System;
using System.IO;
using System.Text;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Infrastructure;

public class FileAppLogger : IAppLogger, IDisposable
{
    private readonly IAppPaths _paths;
    private readonly IClock _clock;
    private readonly object _lock = new object();
    private StreamWriter? _writer;
    private string? _currentDate;
    private bool _disposed;

    public FileAppLogger(IAppPaths paths, IClock clock)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void Debug(string message) => Write("DEBUG", message);

    public void Information(string message) => Write("INFORMATION", message);

    public void Warning(string message) => Write("WARNING", message);

    public void Error(string message) => Write("ERROR", message);

    public void Error(Exception exception, string message)
    {
        var redactedMessage = SecretRedactor.Redact(message);
        var redactedExceptionType = SecretRedactor.Redact(exception?.GetType().FullName ?? "Unknown");
        var redactedExceptionMessage = SecretRedactor.Redact(exception?.Message ?? "No message");
        var redactedStackTrace = SecretRedactor.Redact(exception?.StackTrace ?? "No stack trace");

        Write("ERROR", $"{redactedMessage}{Environment.NewLine}Exception: {redactedExceptionType}{Environment.NewLine}Message: {redactedExceptionMessage}{Environment.NewLine}StackTrace: {redactedStackTrace}");
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;
            _disposed = true;
            _writer?.Dispose();
            _writer = null;
        }
    }

    private void Write(string level, string message)
    {
        var now = _clock.UtcNow;
        var dateKey = now.ToString("yyyyMMdd");

        lock (_lock)
        {
            if (_disposed)
                return;

            var redacted = SecretRedactor.Redact(message);

            if (_writer == null || _currentDate != dateKey)
            {
                _writer?.Dispose();
                _paths.EnsureCreated();
                var logFile = Path.Combine(_paths.LogsDirectory, $"plugin-{dateKey}.log");
                _writer = new StreamWriter(logFile, append: true, Encoding.UTF8);
                _currentDate = dateKey;
            }

            var timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            _writer.WriteLine($"{timestamp} [{level}] {redacted}");
            _writer.Flush();
        }
    }
}

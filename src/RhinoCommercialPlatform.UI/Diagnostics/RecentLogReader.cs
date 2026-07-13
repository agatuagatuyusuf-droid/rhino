using System;
using System.Collections.Generic;
using System.IO;

namespace RhinoCommercialPlatform.UI.Diagnostics;

public sealed class RecentLogReader
{
    private const int DefaultMaxLines = 200;
    private const long MaxBytes = 256 * 1024; // 256 KB

    public static List<string> ReadLastLines(string logFilePath, int maxLines = DefaultMaxLines)
    {
        var lines = new List<string>();

        if (string.IsNullOrWhiteSpace(logFilePath))
            return lines;

        if (!File.Exists(logFilePath))
        {
            lines.Add("[日志文件不存在]");
            return lines;
        }

        try
        {
            var fileInfo = new FileInfo(logFilePath);
            if (fileInfo.Length == 0)
            {
                lines.Add("[日志文件为空]");
                return lines;
            }

            // If the file is huge, read only the last MaxBytes
            long startOffset;
            if (fileInfo.Length > MaxBytes)
            {
                startOffset = fileInfo.Length - MaxBytes;
            }
            else
            {
                startOffset = 0;
            }

            using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                if (startOffset > 0)
                    stream.Seek(startOffset, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);

                        // Keep only the last maxLines
                        if (lines.Count > maxLines * 2)
                        {
                            lines.RemoveRange(0, lines.Count - maxLines);
                        }
                    }
                }
            }

            // Trim to maxLines
            if (lines.Count > maxLines)
            {
                lines.RemoveRange(0, lines.Count - maxLines);
            }
        }
        catch (IOException ex)
        {
            lines.Clear();
            lines.Add($"[读取日志文件失败: {ex.Message}]");
        }
        catch (UnauthorizedAccessException ex)
        {
            lines.Clear();
            lines.Add($"[无权限读取日志文件: {ex.Message}]");
        }

        return lines;
    }
}

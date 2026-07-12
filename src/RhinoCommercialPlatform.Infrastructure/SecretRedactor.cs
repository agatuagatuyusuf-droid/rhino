using System;
using System.Text.RegularExpressions;

namespace RhinoCommercialPlatform.Infrastructure;

public static class SecretRedactor
{
    private static readonly Regex SecretPattern = new Regex(
        @"(?i)(token|access_token|refresh_token|license|license_key|activation_code|password|authorization)\s*[=:]\s*\S+",
        RegexOptions.Compiled);

    public static string Redact(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        return SecretPattern.Replace(message, match =>
        {
            var colonIndex = match.Value.IndexOfAny(new[] { '=', ':' });
            if (colonIndex < 0)
                return match.Value;

            return match.Value.Substring(0, colonIndex + 1) + " [REDACTED]";
        });
    }
}

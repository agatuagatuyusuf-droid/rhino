using System;
using System.Text.RegularExpressions;

namespace RhinoCommercialPlatform.Infrastructure;

public static class SecretRedactor
{
    private const string KeyPattern =
        "token|access_token|refresh_token|license|license_key|" +
        "activation_code|password|authorization";

    private static readonly TimeSpan MatchTimeout =
        TimeSpan.FromMilliseconds(250);

    private static readonly Regex AuthorizationSchemePattern =
        new Regex(
            $@"(?<prefix>(?<![\w])[""']?authorization[""']?\s*[:=]\s*)" +
            @"(?:bearer|basic)\s+(?<value>[^\s,;]+)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant,
            MatchTimeout);

    private static readonly Regex QuotedValuePattern =
        new Regex(
            $@"(?<prefix>(?<![\w])[""']?(?:{KeyPattern})[""']?\s*[:=]\s*)" +
            @"(?<quote>[""'])(?<value>.*?)(?<end>\k<quote>)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant,
            MatchTimeout);

    private static readonly Regex UnquotedValuePattern =
        new Regex(
            $@"(?<prefix>(?<![\w])[""']?(?:{KeyPattern})[""']?\s*[:=]\s*)" +
            @"(?<value>[^\s,;]+)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant,
            MatchTimeout);

    public static string Redact(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var redacted = AuthorizationSchemePattern.Replace(
            message,
            match => match.Groups["prefix"].Value + "[REDACTED]");

        redacted = QuotedValuePattern.Replace(
            redacted,
            match =>
                match.Groups["prefix"].Value +
                match.Groups["quote"].Value +
                "[REDACTED]" +
                match.Groups["end"].Value);

        return UnquotedValuePattern.Replace(
            redacted,
            match => match.Groups["prefix"].Value + "[REDACTED]");
    }
}

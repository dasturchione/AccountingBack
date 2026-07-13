using System.Text.RegularExpressions;

namespace Integration.Edocs;

public sealed partial class EdocsResponseSanitizer
{
    private const int MaximumDiagnosticLength = 256;

    [GeneratedRegex("(?i)(token|pkcs7(?:_64)?|password|cookie|set-cookie|authorization)\\s*[:=]\\s*(?:\\\"[^\\\"]*\\\"|[^\\s,;}}\\]]+)")]
    private static partial Regex SecretAssignment();

    [GeneratedRegex("(?i)bearer\\s+[A-Za-z0-9._~+/-]+")]
    private static partial Regex BearerToken();

    public string SanitizeForLog(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        var redacted = SecretAssignment().Replace(body, "$1=[REDACTED]");
        redacted = BearerToken().Replace(redacted, "Bearer [REDACTED]");
        return redacted.Length <= MaximumDiagnosticLength
            ? redacted
            : redacted[..MaximumDiagnosticLength] + "…";
    }

    public string SafeClientMessage() => "E-DOCS request failed.";
}

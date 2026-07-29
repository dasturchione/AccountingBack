using System.Text.RegularExpressions;

namespace SharedKernel.Security;

/// <summary>
/// Tashqi tizim javobini log yoki istisno matniga qo'shishdan oldin undagi
/// maxfiy qiymatlarni tozalaydi.
/// </summary>
public static class SensitiveDataRedactor
{
    public const string Placeholder = "[REDACTED]";

    private static readonly Regex SensitivePattern = new(
        "(?i)(authorization|api[_-]?key|access[_-]?token|token|password)\\s*[:=]\\s*(?:\\\"[^\\\"]*\\\"|Bearer\\s+[^,\\s}]+|[^,\\s}]+)");

    /// <param name="value">Tozalanadigan matn.</param>
    /// <param name="knownSecret">
    /// Ma'lum maxfiy qiymat (masalan API kalit). Bo'sh bo'lmasa, matndagi barcha
    /// uchrashi to'g'ridan-to'g'ri almashtiriladi.
    /// </param>
    public static string Redact(string value, string? knownSecret = null)
    {
        if (!string.IsNullOrWhiteSpace(knownSecret))
            value = value.Replace(knownSecret, Placeholder, StringComparison.Ordinal);

        return SensitivePattern.Replace(value, $"$1={Placeholder}");
    }
}

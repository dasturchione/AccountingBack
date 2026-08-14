using System.Text;

namespace SharedKernel.Text;

public static class Utf8MojibakeNormalizer
{
    public const int ProviderProductNameMaxLength = 500;

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value)
            || (!value.Contains('Ð') && !value.Contains('Ñ')))
        {
            return value;
        }

        var bytes = new byte[value.Length];
        for (var index = 0; index < value.Length; index++)
        {
            if (!TryGetWindows1252Byte(value[index], out bytes[index]))
                return value;
        }

        var repaired = Encoding.UTF8.GetString(bytes);
        return repaired.Contains('\uFFFD') || !repaired.Any(character => character is >= '\u0400' and <= '\u052F')
            ? value
            : repaired;
    }

    public static bool TryNormalizeProviderProductName(string? value, out string? normalized)
    {
        var repaired = Normalize(value);
        if (string.IsNullOrWhiteSpace(repaired))
        {
            normalized = null;
            return true;
        }

        var builder = new StringBuilder(repaired.Length);
        var pendingSpace = false;
        for (var index = 0; index < repaired.Length; index++)
        {
            var character = repaired[index];
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }
            if (character == '\0' || character == '\uFFFD' || char.IsControl(character))
            {
                normalized = null;
                return false;
            }
            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= repaired.Length || !char.IsLowSurrogate(repaired[index + 1]))
                {
                    normalized = null;
                    return false;
                }
            }
            else if (char.IsLowSurrogate(character)
                     && (index == 0 || !char.IsHighSurrogate(repaired[index - 1])))
            {
                normalized = null;
                return false;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }
            builder.Append(character);
            if (builder.Length > ProviderProductNameMaxLength)
            {
                normalized = null;
                return false;
            }
        }

        normalized = builder.Length == 0 ? null : builder.ToString();
        return true;
    }

    private static bool TryGetWindows1252Byte(char value, out byte result)
    {
        if (value <= '\u00FF')
        {
            result = (byte)value;
            return true;
        }

        result = value switch
        {
            '\u20AC' => 0x80,
            '\u201A' => 0x82,
            '\u0192' => 0x83,
            '\u201E' => 0x84,
            '\u2026' => 0x85,
            '\u2020' => 0x86,
            '\u2021' => 0x87,
            '\u02C6' => 0x88,
            '\u2030' => 0x89,
            '\u0160' => 0x8A,
            '\u2039' => 0x8B,
            '\u0152' => 0x8C,
            '\u017D' => 0x8E,
            '\u2018' => 0x91,
            '\u2019' => 0x92,
            '\u201C' => 0x93,
            '\u201D' => 0x94,
            '\u2022' => 0x95,
            '\u2013' => 0x96,
            '\u2014' => 0x97,
            '\u02DC' => 0x98,
            '\u2122' => 0x99,
            '\u0161' => 0x9A,
            '\u203A' => 0x9B,
            '\u0153' => 0x9C,
            '\u017E' => 0x9E,
            '\u0178' => 0x9F,
            _ => 0
        };

        return result != 0;
    }
}

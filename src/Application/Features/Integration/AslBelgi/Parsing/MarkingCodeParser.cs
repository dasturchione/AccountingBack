namespace Application.Features.Integration.AslBelgi.Parsing;

public sealed record MarkingCodeParts(string Gtin, string SerialNumber, string CheckKey, string CheckCode);

/// <summary>
/// Parses the "maishiy texnika" (household appliances) marking code layout:
/// (01)GTIN14 + (21)SerialNumber20 + (91)CheckKey4 + (92)CheckCode44 — 90 characters,
/// concatenated without the ASCII 29 (GS) separator used by other product groups.
/// </summary>
public static class MarkingCodeParser
{
    private const int GtinLength = 14;
    private const int SerialNumberLength = 20;
    private const int CheckKeyLength = 4;
    private const int CheckCodeLength = 44;
    private const string GtinPrefix = "01";
    private const string SerialNumberPrefix = "21";
    private const string CheckKeyPrefix = "91";
    private const string CheckCodePrefix = "92";

    public const int ExpectedLength =
        2 + GtinLength +
        2 + SerialNumberLength +
        2 + CheckKeyLength +
        2 + CheckCodeLength;

    public static MarkingCodeParts Parse(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new MarkingCodeFormatException("Marking code must not be null or empty.");

        if (code.Length != ExpectedLength)
            throw new MarkingCodeFormatException(
                $"Marking code must be exactly {ExpectedLength} characters for the household appliances format ((01)GTIN14+(21)SN20+(91)CheckKey4+(92)CheckCode44); got {code.Length}.");

        var offset = 0;
        RequirePrefix(code, GtinPrefix, ref offset);
        var gtin = Take(code, GtinLength, ref offset);
        RequirePrefix(code, SerialNumberPrefix, ref offset);
        var serialNumber = Take(code, SerialNumberLength, ref offset);
        RequirePrefix(code, CheckKeyPrefix, ref offset);
        var checkKey = Take(code, CheckKeyLength, ref offset);
        RequirePrefix(code, CheckCodePrefix, ref offset);
        var checkCode = Take(code, CheckCodeLength, ref offset);

        return new MarkingCodeParts(gtin, serialNumber, checkKey, checkCode);
    }

    /// <summary>Reassembles the 90-character code string from its parts — the inverse of <see cref="Parse"/>.</summary>
    public static string Compose(MarkingCodeParts parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        if (parts.Gtin.Length != GtinLength)
            throw new MarkingCodeFormatException($"Gtin must be exactly {GtinLength} characters; got {parts.Gtin.Length}.");
        if (parts.SerialNumber.Length != SerialNumberLength)
            throw new MarkingCodeFormatException($"SerialNumber must be exactly {SerialNumberLength} characters; got {parts.SerialNumber.Length}.");
        if (parts.CheckKey.Length != CheckKeyLength)
            throw new MarkingCodeFormatException($"CheckKey must be exactly {CheckKeyLength} characters; got {parts.CheckKey.Length}.");
        if (parts.CheckCode.Length != CheckCodeLength)
            throw new MarkingCodeFormatException($"CheckCode must be exactly {CheckCodeLength} characters; got {parts.CheckCode.Length}.");

        return GtinPrefix + parts.Gtin + SerialNumberPrefix + parts.SerialNumber + CheckKeyPrefix + parts.CheckKey + CheckCodePrefix + parts.CheckCode;
    }

    /// <summary>
    /// Builds the IDENTIFICATION-ONLY portion of the code — (01)GTIN14+(21)SerialNumber20,
    /// without the (91)/(92) verification part. Some CRPT endpoints (e.g. aggregation,
    /// §5.3 <c>aggregationUnits[].codes</c> / <c>unitSerialNumber</c>) expect this shorter
    /// form rather than the full <see cref="Compose"/> output.
    /// </summary>
    public static string ComposeIdentification(MarkingCodeParts parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        if (parts.Gtin.Length != GtinLength)
            throw new MarkingCodeFormatException($"Gtin must be exactly {GtinLength} characters; got {parts.Gtin.Length}.");
        if (parts.SerialNumber.Length != SerialNumberLength)
            throw new MarkingCodeFormatException($"SerialNumber must be exactly {SerialNumberLength} characters; got {parts.SerialNumber.Length}.");

        return GtinPrefix + parts.Gtin + SerialNumberPrefix + parts.SerialNumber;
    }

    public static bool TryParse(string code, out MarkingCodeParts? parts)
    {
        try
        {
            parts = Parse(code);
            return true;
        }
        catch (MarkingCodeFormatException)
        {
            parts = null;
            return false;
        }
    }

    private static void RequirePrefix(string code, string expectedPrefix, ref int offset)
    {
        var actual = code.Substring(offset, expectedPrefix.Length);
        if (actual != expectedPrefix)
            throw new MarkingCodeFormatException(
                $"Expected application identifier '{expectedPrefix}' at position {offset}, found '{actual}'.");

        offset += expectedPrefix.Length;
    }

    private static string Take(string code, int length, ref int offset)
    {
        var value = code.Substring(offset, length);
        offset += length;
        return value;
    }
}

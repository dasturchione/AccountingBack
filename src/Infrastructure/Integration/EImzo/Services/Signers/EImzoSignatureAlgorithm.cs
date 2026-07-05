namespace Integration.EImzo.Services.Signers;

internal sealed class EImzoSignatureAlgorithm
{
    private EImzoSignatureAlgorithm(string rawValue, string? name, string? oid)
    {
        RawValue = rawValue;
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Oid = string.IsNullOrWhiteSpace(oid) ? null : oid.Trim();
    }

    public string RawValue { get; }
    public string? Name { get; }
    public string? Oid { get; }

    public string DisplayValue => Oid ?? RawValue;

    public static EImzoSignatureAlgorithm Parse(string? signatureAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(signatureAlgorithm))
            return new EImzoSignatureAlgorithm("Unknown", null, null);

        var rawValue = signatureAlgorithm.Trim();
        var openIndex = rawValue.LastIndexOf('(');
        var closeIndex = rawValue.LastIndexOf(')');

        if (openIndex >= 0 && closeIndex > openIndex)
        {
            var candidateOid = rawValue[(openIndex + 1)..closeIndex].Trim();
            if (LooksLikeOid(candidateOid))
            {
                var name = rawValue[..openIndex].Trim();
                return new EImzoSignatureAlgorithm(rawValue, name, candidateOid);
            }
        }

        return LooksLikeOid(rawValue)
            ? new EImzoSignatureAlgorithm(rawValue, null, rawValue)
            : new EImzoSignatureAlgorithm(rawValue, rawValue, null);
    }

    private static bool LooksLikeOid(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Split('.').All(part => int.TryParse(part, out _));
    }
}

namespace Integration.EImzo.Configs;

public sealed class EImzoSettings
{
    public string? EImzoServerUrl { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = null!;
    public string CertificatePassword { get; set; } = null!;
    public string? TimestampUrl { get; set; } = string.Empty;
    public bool ValidateCertificateChain { get; set; } = false;
}

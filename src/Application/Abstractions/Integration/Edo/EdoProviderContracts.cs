using System.Text.Json.Serialization;

namespace Application.Abstractions.Integration.Edo;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdoProviderCode
{
    DIDOX,
    FAKTURA,
    EDOCS
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdoCapabilityStatus
{
    SUPPORTED,
    PARTIAL,
    UNKNOWN,
    NOT_SUPPORTED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdoAuthMode
{
    EImzo,
    OAuthPassword,
    Password,
    Other
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdoSigningMode
{
    TimestampedSignature,
    PreparedPkcs7,
    Hash,
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdoCapabilityKind
{
    AuthChallenge,
    AuthComplete,
    CreateFactura,
    SignOutbox,
    ListInbox,
    RejectInbox,
    GetFile,
    GetOutboxStatus,
    GetInboxStatus
}

public sealed class EdoCapabilityDto
{
    public EdoCapabilityKind Kind { get; init; }
    public EdoCapabilityStatus Status { get; init; }
}

public sealed class EdoProviderCapabilityDto
{
    public EdoProviderCode ProviderCode { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    // Ushbu collection capability ma'lumotlarining yagona authoritative manbasi.
    public IReadOnlyCollection<EdoCapabilityDto> Capabilities { get; init; } = [];
    public IReadOnlyCollection<EdoAuthMode> AuthModes { get; init; } = [];
    public IReadOnlyCollection<EdoSigningMode> SigningModes { get; init; } = [];
}

public sealed class EdoActiveProviderRequestDto
{
    public EdoProviderCode ProviderCode { get; init; }
}

public sealed class EdoActiveProviderDto
{
    public EdoProviderCode ProviderCode { get; init; }
}

public sealed class EdoProviderDto
{
    public EdoProviderCode ProviderCode { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsConfigured { get; init; }
    public bool IsLocalConfigurationOnly { get; init; } = true;
    public IReadOnlyCollection<EdoAuthMode> AuthModes { get; init; } = [];
    public IReadOnlyCollection<EdoSigningMode> SigningModes { get; init; } = [];
    public IReadOnlyCollection<EdoCapabilityDto> Capabilities { get; init; } = [];
}

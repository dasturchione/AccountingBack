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
    GetInboxStatus,
    ListOutbox,
    ListDrafts,
    ListAll,
    GetDetail,
    Summary,
    SearchFilter,
    Delete,
    Restore,
    Export,
    Marking
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

public sealed class EdoProviderSelectionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

public sealed class EdoCapabilitiesResponseDto
{
    public EdoProviderCode Provider { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoAuthMode> AuthModes { get; init; } = [];
    public IReadOnlyCollection<EdoSigningMode> SigningModes { get; init; } = [];
    public EdoFrontendCapabilitiesDto Capabilities { get; init; } = new();
    public IReadOnlyCollection<EdoCategoryCapabilityDto> CategoryCapabilities { get; init; } = [];
    public IReadOnlyCollection<EdoStatusCapabilityDto> StatusCapabilities { get; init; } = [];
    [JsonIgnore]
    public IReadOnlyCollection<EdoCapabilityDto> RawCapabilities { get; init; } = [];
}

public sealed class EdoCategoryCapabilityDto
{
    public EdoDocumentCategory Category { get; init; }
    public EdoCapabilityStatus Capability { get; init; }
}

public sealed class EdoStatusCapabilityDto
{
    public EdoDocumentStatusCode Status { get; init; }
    public EdoCapabilityStatus Capability { get; init; }
}

public sealed class EdoFrontendCapabilitiesDto
{
    public EdoCapabilityStatus CanListInbox { get; init; }
    public EdoCapabilityStatus CanListOutbox { get; init; }
    public EdoCapabilityStatus CanListDrafts { get; init; }
    public EdoCapabilityStatus CanListAll { get; init; }
    public EdoCapabilityStatus CanAggregateAll { get; init; }
    public EdoCapabilityStatus CanGetDetail { get; init; }
    public EdoCapabilityStatus CanGetFile { get; init; }
    public EdoCapabilityStatus CanGetStatus { get; init; }
    public EdoCapabilityStatus CanCreate { get; init; }
    public EdoCapabilityStatus CanSign { get; init; }
    public EdoCapabilityStatus CanReject { get; init; }
    public EdoCapabilityStatus CanDelete { get; init; }
    public EdoCapabilityStatus CanRestore { get; init; }
    public EdoCapabilityStatus CanExport { get; init; }
    public EdoCapabilityStatus CanMarking { get; init; }
}

public static class EdoProviderFrontendCatalog
{
    public static IReadOnlyList<(int Id, EdoProviderCode Code)> OrderedProviders { get; } =
    [
        (1, EdoProviderCode.DIDOX),
        (2, EdoProviderCode.EDOCS),
        (3, EdoProviderCode.FAKTURA)
    ];

    public static int GetId(EdoProviderCode providerCode) =>
        OrderedProviders
            .First(item => item.Code == providerCode)
            .Id;
}

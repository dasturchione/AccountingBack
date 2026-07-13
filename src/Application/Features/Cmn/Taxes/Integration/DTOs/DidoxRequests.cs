using System.Text.Json.Serialization;

namespace Application.Features.Cmn.Taxes.Integration.DTOs;

// Submit uses the typed Didox ЭСФ model directly as the create request body (see DidoxInvoiceRequest).
// Status and cancel keep their lightweight request shapes below.

/// <summary>Sign request body: { "signature": "&lt;pkcs7 timestamp b64&gt;" } (signature from frontend E-IMZO).</summary>
public sealed class DidoxSignRequest
{
    [JsonPropertyName("signature")]
    public string Signature { get; init; } = string.Empty;
}

public sealed class StatusDidoxRequest
{
    public string Operation { get; init; } = "status";
    public string? ProviderCode { get; init; }
    public int? OrganizationId { get; init; }
    public string? DocumentNumber { get; init; }
    public string? ExternalDocumentId { get; init; }
    public StatusDidoxPayload? Payload { get; init; }
}

public sealed class StatusDidoxPayload
{
    public string? Query { get; init; }
}

public sealed class CancelDidoxRequest
{
    public string Operation { get; init; } = "cancel";
    public string? ProviderCode { get; init; }
    public int? OrganizationId { get; init; }
    public string? DocumentNumber { get; init; }
    public string? ExternalDocumentId { get; init; }
    public CancelDidoxPayload? Payload { get; init; }
}

public sealed class CancelDidoxPayload
{
    public string? Reason { get; init; }
}

using System.Text.Json.Nodes;

namespace Integration.AslBelgi.DTOs;

public abstract class AslBelgiResponseResult
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public JsonArray? Errors { get; set; }
    public AslBelgiResponseEnvelope? Envelope { get; set; }
}

public sealed class AslBelgiCheckApiKeyResponse : AslBelgiResponseResult
{
    public bool IsValid { get; set; }
    public string? ApiKey { get; set; }
}

public sealed class AslBelgiSubmitResponse : AslBelgiResponseResult
{
    public bool IsSuccess { get; set; }
    public string? OrderId { get; set; }
    public JsonNode? Data { get; set; }
}

public sealed class AslBelgiDocumentResponse : AslBelgiResponseResult
{
    public string? DocumentId { get; set; }
    public string? Content { get; set; }
}

public sealed class AslBelgiRefreshApiKeyResponse : AslBelgiResponseResult
{
    public string? ApiKey { get; set; }
    public string? Id { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
    public string? Label { get; set; }
    public JsonNode? Data { get; set; }
}

public sealed class AslBelgiRefreshApiKeyRequest
{
    public string? Tin { get; set; }
    public string? ApiKey { get; set; }
    public string? Id { get; set; }
}

public sealed class AslBelgiResponseEnvelope
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public JsonArray? Errors { get; set; }
    public JsonNode? Data { get; set; }
    public JsonArray? Items { get; set; }
}

public sealed class AslBelgiStatusResponse
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public JsonArray? Errors { get; set; }
    public AslBelgiResponseEnvelope? Envelope { get; set; }
    public string? Id { get; set; }
    public JsonNode? Data { get; set; }
    public string? Details { get; set; }
}

public sealed class AslBelgiResponseEnvelope<T>
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public JsonArray? Errors { get; set; }
    public T? Data { get; set; }
    public T[]? Items { get; set; }
}

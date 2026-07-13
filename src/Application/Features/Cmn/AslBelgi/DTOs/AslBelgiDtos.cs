using System.Text.Json.Nodes;

namespace Application.Features.Cmn.AslBelgi.DTOs;

public class AslBelgiEnvelopeDto
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }
    public JsonNode? Data { get; set; }
    public JsonArray? Items { get; set; }
}

public sealed class AslBelgiEnvelopeDto<T>
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }
    public T? Data { get; set; }
    public T[]? Items { get; set; }
}

public class AslBelgiResultDto : AslBelgiEnvelopeDto
{
    public AslBelgiEnvelopeDto? Envelope { get; set; }
}

public sealed class AslBelgiCheckApiKeyResponseDto : AslBelgiResultDto
{
    public bool IsValid { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
}

public sealed class AslBelgiRefreshApiKeyRequestDto
{
    public string? Tin { get; set; }

    public string? ApiKey { get; set; }

    public string? Id { get; set; }
}

public sealed class AslBelgiRefreshApiKeyResponseDto : AslBelgiResultDto
{
    public string? ApiKey { get; set; }
    public string? Id { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
    public string? Label { get; set; }
}

public sealed class AslBelgiDocumentResponseDto : AslBelgiResultDto
{
    public string? DocumentId { get; set; }
    public string? Content { get; set; }
}

public sealed class AslBelgiStatusResponseDto : AslBelgiResultDto
{
    public string? Id { get; set; }
    public string? Details { get; set; }
}

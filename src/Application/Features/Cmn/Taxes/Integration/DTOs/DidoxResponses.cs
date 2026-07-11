using System.Text.Json;

namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class SubmitDidoxResponse
{
    public string? ExternalDocumentId { get; init; }
    public string? StatusCode { get; init; }
    public string? Code { get; init; }
    public string? StatusName { get; init; }
    public bool? IsSuccessful { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public JsonElement? Errors { get; init; }
    public string? Status { get; init; }
    public DidoxResponseEnvelope? Data { get; init; }
    public IReadOnlyList<JsonElement>? Items { get; init; }
}

public sealed class StatusDidoxResponse
{
    public string? ExternalDocumentId { get; init; }
    public string? StatusCode { get; init; }
    public string? Code { get; init; }
    public string? StatusName { get; init; }
    public bool? IsSuccessful { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public JsonElement? Errors { get; init; }
    public string? Status { get; init; }
    public DidoxResponseEnvelope? Data { get; init; }
    public IReadOnlyList<JsonElement>? Items { get; init; }
}

public sealed class CancelDidoxResponse
{
    public string? ExternalDocumentId { get; init; }
    public string? StatusCode { get; init; }
    public string? Code { get; init; }
    public string? StatusName { get; init; }
    public bool? IsSuccessful { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public JsonElement? Errors { get; init; }
    public string? Status { get; init; }
    public DidoxResponseEnvelope? Data { get; init; }
    public IReadOnlyList<JsonElement>? Items { get; init; }
}

public sealed class DidoxResponseEnvelope
{
    public string? ExternalDocumentId { get; init; }
    public string? StatusCode { get; init; }
    public string? Status { get; init; }
    public string? Code { get; init; }
    public string? StatusName { get; init; }
    public string? Message { get; init; }
    public JsonElement? Content { get; init; }
}

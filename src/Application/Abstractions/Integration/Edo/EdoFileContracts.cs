namespace Application.Abstractions.Integration.Edo;

public sealed class EdoFileDto
{
    public long Id { get; init; }
    public long DocumentId { get; init; }
    public string? ProviderFileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public long Length { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}

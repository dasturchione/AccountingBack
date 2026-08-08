namespace WebApi.Contracts;

public sealed class EdoPublicProblemDetailsDto
{
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}

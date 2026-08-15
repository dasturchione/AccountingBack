using SharedKernel.Results;

namespace Application.Features.AiAssistant;

public interface IAiOrganizationContextResolver
{
    Task<Result<AiOrganizationContext>> ResolveAsync(
        AiOrganizationContextRequest? request,
        CancellationToken ct = default);
}

public sealed record AiOrganizationContext(
    int OrganizationId,
    string Name,
    string Inn);

public sealed record AiOrganizationContextRequest(
    int? OrganizationId = null,
    string? Name = null,
    string? Inn = null);

using SharedKernel.Results;

namespace Application.Features.AiAssistant;

public interface IAiToolExecutionGuard
{
    Task<Result<AiOrganizationContext>> ValidateAsync(
        AiOrganizationContext organizationContext,
        CancellationToken ct = default);
}

public sealed class AiToolExecutionGuard(IAiOrganizationContextResolver organizationContextResolver)
    : IAiToolExecutionGuard
{
    public async Task<Result<AiOrganizationContext>> ValidateAsync(
        AiOrganizationContext organizationContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(organizationContext);

        var resolved = await organizationContextResolver.ResolveAsync(
            new AiOrganizationContextRequest(
                organizationContext.OrganizationId,
                organizationContext.Name,
                organizationContext.Inn),
            ct);

        return resolved;
    }
}

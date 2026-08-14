using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.AiAssistant;

public sealed class OrganizationContextResolver(
    IUserContext userContext,
    IQueryRepository<Organization> organizationQuery) : IAiOrganizationContextResolver
{
    public async Task<Result<AiOrganizationContext>> ResolveAsync(
        AiOrganizationContextRequest? request,
        CancellationToken ct = default)
    {
        var allowedOrganizationIds = userContext.AllowedOrganizationIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (allowedOrganizationIds.Length == 0)
            return Result.Failure<AiOrganizationContext>(
                AiAssistantErrors.OrganizationContextUnavailable(userContext.LanguageId));

        var requestedName = Normalize(request?.Name);
        var requestedInn = Normalize(request?.Inn);
        var hasExplicitReference = request?.OrganizationId is not null
            || requestedName is not null
            || requestedInn is not null;

        if (request?.OrganizationId is <= 0)
            return Result.Failure<AiOrganizationContext>(
                AiAssistantErrors.OrganizationContextUnavailable(userContext.LanguageId));

        var organizationId = request?.OrganizationId
            ?? (!hasExplicitReference ? userContext.OrganizationId : null);

        if (organizationId is not null && !allowedOrganizationIds.Contains(organizationId.Value))
            return Result.Failure<AiOrganizationContext>(
                AiAssistantErrors.OrganizationContextUnavailable(userContext.LanguageId));

        if (organizationId is null && requestedName is null && requestedInn is null)
            return Result.Failure<AiOrganizationContext>(
                AiAssistantErrors.OrganizationContextUnavailable(userContext.LanguageId));

        var normalizedName = requestedName?.ToUpperInvariant();
        var specification = new QuerySpecification<Organization, AiOrganizationContext>
        {
            Criteria = organization =>
                allowedOrganizationIds.Contains(organization.Id)
                && organization.StateId == StateIdConst.ACTIVE
                && (!organizationId.HasValue || organization.Id == organizationId.Value)
                && (normalizedName == null
                    || organization.ShortName.ToUpper() == normalizedName
                    || organization.FullName.ToUpper() == normalizedName)
                && (requestedInn == null || organization.Inn == requestedInn),
            Selector = organization => new AiOrganizationContext(
                organization.Id,
                organization.FullName,
                organization.Inn)
        };

        var resolved = await organizationQuery.GetAsync(specification, ct);

        return resolved is null
            ? Result.Failure<AiOrganizationContext>(
                AiAssistantErrors.OrganizationContextUnavailable(userContext.LanguageId))
            : Result.Success(resolved);
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

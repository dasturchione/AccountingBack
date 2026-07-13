using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Integration;

public sealed class OrganizationScopeResolver(
    IUserContext userContext,
    IQueryRepository<UserOrganization> membershipQuery,
    IQueryRepository<Organization> organizationQuery) : IOrganizationScopeResolver
{
    public async Task<Result<OrganizationScope>> ResolveAsync(
        Provider provider,
        string? entityId = null,
        CancellationToken ct = default)
    {
        if (!Enum.IsDefined(provider))
            return Result.Failure<OrganizationScope>(Error.Problem(
                "Integration.ProviderInvalid", "The provider is not supported."));

        if (userContext.Id is not int userId || userId <= 0)
            return Result.Failure<OrganizationScope>(Error.Unauthorized(
                "Integration.UserRequired", "An authenticated user is required."));

        if (userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<OrganizationScope>(Error.Forbidden(
                "Integration.OrganizationRequired", "A current organization is required."));

        var membership = await membershipQuery.GetAsync(new QuerySpecification<UserOrganization>
        {
            IgnoreQueryFilters = true,
            Criteria = x => x.UserId == userId
                && x.OrganizationId == organizationId
                && x.StateId == StateIdConst.ACTIVE
        }, ct);

        if (membership is null)
            return Result.Failure<OrganizationScope>(Error.Forbidden(
                "Integration.OrganizationForbidden", "The user is not an active member of this organization."));

        var organization = await organizationQuery.GetAsync(new QuerySpecification<Organization>
        {
            IgnoreQueryFilters = true,
            Criteria = x => x.Id == organizationId
        }, ct);

        if (organization is null)
            return Result.Failure<OrganizationScope>(Error.NotFound(
                "Integration.OrganizationNotFound", "The current organization was not found."));

        var externalTin = NormalizeTin(organization.Inn);
        if (externalTin.Length == 0)
            return Result.Failure<OrganizationScope>(Error.Business(
                "Integration.OrganizationTinMissing", "The current organization has no valid TIN."));

        var normalizedEntityId = provider == Provider.EDocs
            ? ProviderScopeCanonicalizer.NormalizeEdocsEntityId(entityId)
            : string.IsNullOrWhiteSpace(entityId) ? null : entityId.Trim();

        return Result.Success(new OrganizationScope(
            organizationId,
            provider,
            externalTin,
            normalizedEntityId));
    }

    public static string NormalizeTin(string? tin) =>
        tin is null ? string.Empty : new(tin.Where(char.IsDigit).ToArray());
}

using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Platform;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Organizations;

public sealed class OrganizationManagementCore : IOrganizationManagementCore
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly ICommandRepository<Organization> _organizationCommand;
    private readonly IQueryRepository<PlatformTenant> _tenantQuery;

    public OrganizationManagementCore(
        IUserContext userContext,
        IQueryRepository<Organization> organizationQuery,
        ICommandRepository<Organization> organizationCommand,
        IQueryRepository<PlatformTenant> tenantQuery)
    {
        _userContext = userContext;
        _organizationQuery = organizationQuery;
        _organizationCommand = organizationCommand;
        _tenantQuery = tenantQuery;
    }

    public async Task<Result<Organization>> GetOrganizationAsync(
        int organizationId,
        OrganizationManagementOptions options,
        CancellationToken ct = default)
    {
        if (options.Scope == OrganizationManagementScope.Global && !_userContext.HasGlobalAccess)
            return Result.Failure<Organization>(PlatformErrors.GlobalAccessRequired());

        var organization = await _organizationQuery.GetAsync(BuildOrganizationSpec(organizationId, options.IncludeDetails), ct);
        return organization is null
            ? Result.Failure<Organization>(ResolveNotFound(organizationId, options.Scope))
            : Result.Success(organization);
    }

    public async Task<Result> UpdateOrganizationAsync(
        OrganizationManagementUpdateRequest request,
        OrganizationManagementOptions options,
        CancellationToken ct = default)
    {
        var organizationResult = await GetOrganizationAsync(request.OrganizationId, options, ct);
        if (!organizationResult.IsSuccess)
            return Result.Failure(organizationResult.Error);

        var organization = organizationResult.Value;
        var prepared = PrepareUpdateRequest(request, options.TrimInput);

        if (ShouldCheckInnConflict(organization.Inn, prepared.Inn, options.Scope))
        {
            var exists = await _organizationQuery.AnyAsync(
                x => x.Id != request.OrganizationId && x.Inn == prepared.Inn,
                ct);

            if (exists)
                return Result.Failure(ResolveInnConflict(prepared.Inn, options.Scope));
        }

        if (options.ValidateTenantExists)
        {
            var tenantExists = await _tenantQuery.AnyAsync(x => x.Id == prepared.TenantId, ct);
            if (!tenantExists)
                return Result.Failure(PlatformErrors.TenantNotFound(prepared.TenantId));
        }

        organization.ShortName = prepared.ShortName;
        organization.FullName = prepared.FullName;
        organization.Inn = prepared.Inn;
        organization.PhoneNumber = prepared.PhoneNumber;
        organization.RegionId = prepared.RegionId;
        organization.DistrictId = prepared.DistrictId;
        organization.Address = prepared.Address;
        organization.Director = prepared.Director;
        organization.IsParent = prepared.IsParent;
        organization.DefaultLanguageId = prepared.DefaultLanguageId;
        organization.TenantId = prepared.TenantId;
        organization.SetupStatus = string.IsNullOrWhiteSpace(prepared.SetupStatus)
            ? organization.SetupStatus
            : prepared.SetupStatus;
        organization.SetupCompletedAt = prepared.SetupCompletedAt;
        organization.Email = prepared.Email;
        organization.Website = prepared.Website;
        organization.Oked = prepared.Oked;
        organization.StateId = prepared.StateId;

        await _organizationCommand.UpdateAsync(organization, ct);
        return Result.Success();
    }

    private static QuerySpecification<Organization> BuildOrganizationSpec(int organizationId, bool includeDetails)
    {
        var spec = new QuerySpecification<Organization> { Criteria = x => x.Id == organizationId };
        if (includeDetails)
        {
            spec.AddIncludes(builder =>
            {
                builder.Include(x => x.Region);
                builder.Include(x => x.District);
                builder.Include(x => x.State);
                builder.Include(x => x.DefaultLanguage);
            });
        }

        return spec;
    }

    private static OrganizationManagementUpdateRequest PrepareUpdateRequest(
        OrganizationManagementUpdateRequest request,
        bool trimInput) =>
        trimInput
            ? new OrganizationManagementUpdateRequest
            {
                OrganizationId = request.OrganizationId,
                ShortName = request.ShortName.Trim(),
                FullName = request.FullName.Trim(),
                Inn = request.Inn.Trim(),
                PhoneNumber = request.PhoneNumber,
                RegionId = request.RegionId,
                DistrictId = request.DistrictId,
                Address = request.Address,
                Director = request.Director,
                IsParent = request.IsParent,
                DefaultLanguageId = request.DefaultLanguageId,
                TenantId = request.TenantId,
                SetupStatus = request.SetupStatus?.Trim(),
                SetupCompletedAt = request.SetupCompletedAt,
                Email = request.Email,
                Website = request.Website,
                Oked = request.Oked,
                StateId = request.StateId
            }
            : request;

    private static bool ShouldCheckInnConflict(
        string currentInn,
        string requestedInn,
        OrganizationManagementScope scope) =>
        scope == OrganizationManagementScope.Global
            ? !string.Equals(currentInn, requestedInn, StringComparison.OrdinalIgnoreCase)
            : currentInn != requestedInn;

    private Error ResolveNotFound(int organizationId, OrganizationManagementScope scope) =>
        scope == OrganizationManagementScope.Global
            ? PlatformErrors.OrganizationNotFound(organizationId)
            : OrganizationErrors.NotFound(organizationId, _userContext.LanguageId);

    private Error ResolveInnConflict(string inn, OrganizationManagementScope scope) =>
        scope == OrganizationManagementScope.Global
            ? PlatformErrors.OrganizationInnConflict(inn)
            : OrganizationErrors.InnConflict(inn, _userContext.LanguageId);
}

using System.Text.RegularExpressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Platform.Filters;
using Application.Features;
using Application.Features.AuditLogs;
using Application.Features.Users.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.Platform;

public sealed class PlatformService : BaseService, IPlatformService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<PlatformTenant> _tenantQuery;
    private readonly ICommandRepository<PlatformTenant> _tenantCommand;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly IUserManagementCore _userManagementCore;
    private readonly IAuditLogQueryCore _auditLogQueryCore;
    private readonly IDashboardService _dashboardService;
    private readonly IQueryBuilder _queryBuilder;

    public PlatformService(
        IUserContext userContext,
        IQueryRepository<PlatformTenant> tenantQuery,
        ICommandRepository<PlatformTenant> tenantCommand,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<UserOrganization> userOrganizationQuery,
        IUserManagementCore userManagementCore,
        IAuditLogQueryCore auditLogQueryCore,
        IDashboardService dashboardService,
        ILogger<PlatformService> logger,
        IQueryBuilder queryBuilder,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _tenantQuery = tenantQuery;
        _tenantCommand = tenantCommand;
        _organizationQuery = organizationQuery;
        _userQuery = userQuery;
        _userOrganizationQuery = userOrganizationQuery;
        _userManagementCore = userManagementCore;
        _auditLogQueryCore = auditLogQueryCore;
        _dashboardService = dashboardService;
        _queryBuilder = queryBuilder;
    }

    public Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetDashboardAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformDashboardDto>(PlatformErrors.GlobalAccessRequired());

            var statsResult = await _dashboardService.GetStatsAsync(ct);
            if (!statsResult.IsSuccess)
                return Result.Failure<PlatformDashboardDto>(statsResult.Error);

            return new PlatformDashboardDto
            {
                TenantsCount = statsResult.Value.TenantsCount,
                ActiveTenantsCount = statsResult.Value.ActiveTenantsCount,
                InactiveTenantsCount = statsResult.Value.InactiveTenantsCount,
                OrganizationsCount = statsResult.Value.OrganizationsCount,
                ActiveOrganizationsCount = statsResult.Value.ActiveOrganizationsCount,
                InactiveOrganizationsCount = statsResult.Value.InactiveOrganizationsCount,
                UsersCount = statsResult.Value.UsersCount,
                ActiveUsersCount = statsResult.Value.ActiveUsersCount,
                BlockedUsersCount = statsResult.Value.BlockedUsersCount,
                TenantStats = statsResult.Value.TenantStats,
                UserStats = statsResult.Value.UserStats,
                OrganizationStats = statsResult.Value.OrganizationStats,
                ActivityStats = statsResult.Value.ActivityStats,
                NotificationStats = statsResult.Value.NotificationStats,
                SystemStats = statsResult.Value.SystemStats
            };
        });

    public Task<Result<PagedResponse<PlatformTenantDto>>> GetTenantsAsync(PlatformTenantListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantsAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PagedResponse<PlatformTenantDto>>(PlatformErrors.GlobalAccessRequired());

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var spec = new PagedQuerySpecification<PlatformTenant>
            {
                Criteria = tenant =>
                    (string.IsNullOrWhiteSpace(search)
                     || tenant.Name.ToLower().Contains(search)
                     || tenant.Slug.ToLower().Contains(search))
                    && (!filter.StateId.HasValue || tenant.StateId == filter.StateId.Value),
                OrderBy = query => query.OrderBy(tenant => tenant.Id),
                Skip = (page - 1) * pageSize,
                Take = pageSize
            };

            var paged = await _tenantQuery.GetPagedAsync(spec, ct);
            var items = new List<PlatformTenantDto>();
            foreach (var tenant in paged.Items)
                items.Add(await MapTenantAsync(tenant, ct));

            return PagedResponseFactory.Create(new PagedList<PlatformTenantDto>(items, paged.TotalCount), page, pageSize);
        });

    public Task<Result<PlatformTenantDto>> GetTenantByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantByIdAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformTenantDto>(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(id, ct);
            return tenant is null
                ? Result.Failure<PlatformTenantDto>(PlatformErrors.TenantNotFound(id))
                : Result.Success(await MapTenantAsync(tenant, ct));
        });

    public Task<Result<int>> CreateTenantAsync(PlatformTenantCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateTenantAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<int>(PlatformErrors.GlobalAccessRequired());

            var slug = NormalizeSlug(dto.Slug ?? dto.Name);
            if (await _tenantQuery.AnyAsync(x => x.Slug == slug, ct))
                return Result.Failure<int>(PlatformErrors.TenantSlugConflict(slug));

            var now = DateTime.Now;
            var tenant = new PlatformTenant
            {
                Name = dto.Name.Trim(),
                Slug = slug,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now
            };
            await _tenantCommand.CreateAsync(tenant, ct);

            if (dto.User is not null)
            {
                var ownerResult = await _userManagementCore.CreateUserAsync(
                    MapTenantOwnerCreateRequest(dto.User, tenant.Id),
                    UserManagementOptions.ForGlobal(),
                    ct);
                if (!ownerResult.IsSuccess)
                    return Result.Failure<int>(ownerResult.Error);

                tenant.OwnerUserId = ownerResult.Value.UserId;
                tenant.UpdatedDate = now;
                await _tenantCommand.UpdateAsync(tenant, ct);
            }

            return Result.Success(tenant.Id);
        }, ct);

    public Task<Result> UpdateTenantAsync(int id, PlatformTenantUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateTenantAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(id, ct);
            if (tenant is null)
                return Result.Failure(PlatformErrors.TenantNotFound(id));

            var slug = NormalizeSlug(dto.Slug ?? dto.Name);
            if (await _tenantQuery.AnyAsync(x => x.Id != id && x.Slug == slug, ct))
                return Result.Failure(PlatformErrors.TenantSlugConflict(slug));

            if (dto.OwnerUserId.HasValue &&
                !await _userQuery.AnyAsync(x => x.Id == dto.OwnerUserId.Value && x.TenantId == id, ct))
                return Result.Failure(PlatformErrors.UserNotFound(dto.OwnerUserId.Value));

            tenant.Name = dto.Name.Trim();
            tenant.Slug = slug;
            tenant.OwnerUserId = dto.OwnerUserId;
            tenant.StateId = dto.StateId;
            tenant.UpdatedDate = DateTime.Now;
            await _tenantCommand.UpdateAsync(tenant, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<PlatformUserDto>>> GetTenantUsersAsync(int tenantId, PlatformUserListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantUsersAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PagedResponse<PlatformUserDto>>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PagedResponse<PlatformUserDto>>(PlatformErrors.TenantNotFound(tenantId));

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var spec = new PagedQuerySpecification<User, PlatformUserDto>
            {
                Criteria = user =>
                    (string.IsNullOrWhiteSpace(search)
                     || user.UserName.ToLower().Contains(search)
                     || user.PhoneNumber.ToLower().Contains(search)
                     || (user.Email != null && user.Email.ToLower().Contains(search))
                     || user.FirstName.ToLower().Contains(search)
                     || user.LastName.ToLower().Contains(search))
                    && (!filter.RoleId.HasValue || user.RoleId == filter.RoleId.Value)
                    && (!filter.StateId.HasValue || user.StateId == filter.StateId.Value)
                    && (!filter.HasGlobalAccess.HasValue || user.Role.HasGlobalAccess == filter.HasGlobalAccess.Value)
                    && ((!filter.OrganizationId.HasValue && user.TenantId == tenantId)
                        || user.UserOrganizations.Any(membership =>
                            membership.Organization.TenantId == tenantId &&
                            (!filter.OrganizationId.HasValue || membership.OrganizationId == filter.OrganizationId.Value))),
                OrderBy = query => query.OrderBy(user => user.Id),
                Skip = (page - 1) * pageSize,
                Take = pageSize,
                Selector = PlatformUserDtoProjection.Summary
            };

            var paged = await _userQuery.GetPagedAsync(spec, ct);
            return PagedResponseFactory.Create(paged, page, pageSize);
        });

    public Task<Result<PagedResponse<PlatformOrganizationDto>>> GetTenantOrganizationsAsync(int tenantId, PlatformOrganizationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantOrganizationsAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PagedResponse<PlatformOrganizationDto>>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PagedResponse<PlatformOrganizationDto>>(PlatformErrors.TenantNotFound(tenantId));

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var setupStatus = filter.SetupStatus?.Trim().ToLowerInvariant();
            var spec = new PagedQuerySpecification<Organization>
            {
                Criteria = organization =>
                    organization.TenantId == tenantId
                    && (string.IsNullOrWhiteSpace(search)
                        || organization.ShortName.ToLower().Contains(search)
                        || organization.FullName.ToLower().Contains(search)
                        || organization.Inn.ToLower().Contains(search)
                        || (organization.Email != null && organization.Email.ToLower().Contains(search)))
                    && (!filter.RegionId.HasValue || organization.RegionId == filter.RegionId.Value)
                    && (!filter.StateId.HasValue || organization.StateId == filter.StateId.Value)
                    && (string.IsNullOrWhiteSpace(setupStatus) || organization.SetupStatus.ToLower() == setupStatus),
                OrderBy = query => query.OrderBy(organization => organization.Id),
                Skip = (page - 1) * pageSize,
                Take = pageSize
            };
            spec.AddIncludes(builder =>
            {
                builder.Include(x => x.Region);
                builder.Include(x => x.District);
                builder.Include(x => x.State);
                builder.Include(x => x.DefaultLanguage);
            });

            var paged = await _organizationQuery.GetPagedAsync(spec, ct);
            var items = new List<PlatformOrganizationDto>();
            foreach (var organization in paged.Items)
                items.Add(await MapOrganizationAsync(organization, ct));

            return PagedResponseFactory.Create(new PagedList<PlatformOrganizationDto>(items, paged.TotalCount), page, pageSize);
        });

    public Task<Result<PagedResponse<PlatformAuditLogDto>>> GetAuditLogsAsync(PlatformAuditLogListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAuditLogsAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PagedResponse<PlatformAuditLogDto>>(PlatformErrors.GlobalAccessRequired());

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var result = await _auditLogQueryCore.QueryPagedAsync(
                new AuditLogQueryFilter
                {
                    OrganizationId = filter.OrganizationId,
                    UserId = filter.UserId,
                    ChangedUserId = filter.ChangedUserId,
                    EntityType = filter.EntityType,
                    TableName = filter.TableName,
                    EntityId = filter.EntityId,
                    RecordId = filter.RecordId,
                    Action = filter.Action,
                    SearchText = filter.SearchText,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate
                },
                AuditLogQueryOptions.ForGlobal(),
                new AuditLogQueryPagination
                {
                    Page = page,
                    PageSize = pageSize
                },
                ct);

            if (!result.IsSuccess)
                return Result.Failure<PagedResponse<PlatformAuditLogDto>>(result.Error);

            var items = result.Value.Items.Select(CreatePlatformAuditLogDto).ToList();
            return PagedResponseFactory.Create(new PagedList<PlatformAuditLogDto>(items, result.Value.TotalCount), page, pageSize);
        });

    private async Task<PlatformTenant?> GetTenantEntityAsync(int id, CancellationToken ct) =>
        await _tenantQuery.GetAsync(new QuerySpecification<PlatformTenant> { Criteria = x => x.Id == id }, ct);

    private async Task<PlatformTenantDto> MapTenantAsync(PlatformTenant tenant, CancellationToken ct)
    {
        var ownerUserName = tenant.OwnerUserId.HasValue
            ? await _userQuery.GetAsync(new QuerySpecification<User, string>
            {
                Criteria = x => x.Id == tenant.OwnerUserId.Value,
                Selector = x => x.UserName
            }, ct)
            : null;

        var organizationIds = await _organizationQuery.GetAllAsync(new QuerySpecification<Organization, int>
        {
            Criteria = x => x.TenantId == tenant.Id,
            Selector = x => x.Id
        }, ct);
        var membershipUserIds = organizationIds.Count == 0
            ? []
            : await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization, int>
            {
                Criteria = x => organizationIds.Contains(x.OrganizationId) && x.StateId == StateIdConst.ACTIVE,
                Selector = x => x.UserId
            }, ct);
        var tenantUserIds = await _userQuery.GetAllAsync(new QuerySpecification<User, int>
        {
            Criteria = x => x.TenantId == tenant.Id,
            Selector = x => x.Id
        }, ct);

        return new PlatformTenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            OwnerUserId = tenant.OwnerUserId,
            OwnerUserName = ownerUserName,
            StateId = tenant.StateId,
            CreatedDate = tenant.CreatedDate,
            UpdatedDate = tenant.UpdatedDate,
            OrganizationsCount = organizationIds.Count,
            UsersCount = tenantUserIds.Concat(membershipUserIds).Distinct().Count()
        };
    }

    private async Task<PlatformOrganizationDto> MapOrganizationAsync(Organization organization, CancellationToken ct)
    {
        var tenantName = await _tenantQuery.GetAsync(_queryBuilder.For<PlatformTenant>()
            .Where(x => x.Id == organization.TenantId)
            .As(x => x.Name)
            .Build(), ct);
        var usersCount = await CountUserOrganizationsAsync(
            x => x.OrganizationId == organization.Id && x.StateId == StateIdConst.ACTIVE,
            ct);

        return new PlatformOrganizationDto
        {
            Id = organization.Id,
            ShortName = organization.ShortName,
            FullName = organization.FullName,
            Inn = organization.Inn,
            PhoneNumber = organization.PhoneNumber,
            RegionId = organization.RegionId,
            RegionName = organization.Region?.FullName ?? string.Empty,
            DistrictId = organization.DistrictId,
            DistrictName = organization.District?.FullName,
            Address = organization.Address,
            Director = organization.Director,
            IsParent = organization.IsParent,
            StateId = organization.StateId,
            StateName = organization.State?.FullName ?? string.Empty,
            DefaultLanguageId = organization.DefaultLanguageId,
            DefaultLanguageName = organization.DefaultLanguage?.Name,
            TenantId = organization.TenantId,
            TenantName = tenantName,
            SetupStatus = organization.SetupStatus,
            SetupCompletedAt = organization.SetupCompletedAt,
            Email = organization.Email,
            Website = organization.Website,
            Oked = organization.Oked,
            CreatedDate = organization.CreatedDate,
            UsersCount = usersCount
        };
    }

    private static UserManagementCreateRequest MapTenantOwnerCreateRequest(PlatformUserCreateDto dto, int tenantId) =>
        new()
        {
            TenantId = tenantId,
            UserName = dto.UserName,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
            LanguageId = dto.LanguageId,
            EmailVerified = dto.EmailVerified,
            IsPlatformAdmin = dto.IsPlatformAdmin,
            Timezone = dto.Timezone
        };

    private static PlatformAuditLogDto CreatePlatformAuditLogDto(AuditLogQueryItem log) =>
        new()
        {
            Id = log.Id,
            OrganizationId = log.OrganizationId,
            OrganizationName = log.OrganizationName,
            SchemaName = log.SchemaName,
            TableName = log.TableName,
            RecordId = log.RecordId,
            Action = log.Action,
            OldData = log.OldData,
            NewData = log.NewData,
            ChangedUserId = log.ChangedUserId,
            ChangedUserName = log.ChangedUserName,
            RequestId = log.RequestId,
            ClientAddr = log.ClientAddr,
            ApplicationName = log.ApplicationName,
            ChangedDate = log.ChangedDate
        };

    private static string NormalizeSlug(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..12] : slug;
    }

    private async Task<int> CountUserOrganizationsAsync(
        System.Linq.Expressions.Expression<Func<UserOrganization, bool>> criteria,
        CancellationToken ct)
    {
        var page = await _userOrganizationQuery.GetPagedAsync(new PagedQuerySpecification<UserOrganization>
        {
            Criteria = criteria,
            Take = 1
        }, ct);
        return page.TotalCount;
    }
}

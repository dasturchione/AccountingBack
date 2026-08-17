using System.Text.RegularExpressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Platform.Filters;
using Application.Features.AuditLogs;
using Application.Features.Organizations;
using Application.Features.Roles;
using Application.Features.Users.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.Platform;

public sealed partial class PlatformService : BaseService, IPlatformService
{
    private readonly IUserContext _userContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<PlatformTenant> _tenantQuery;
    private readonly ICommandRepository<PlatformTenant> _tenantCommand;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly ICommandRepository<Organization> _organizationCommand;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly ICommandRepository<Role> _roleCommand;
    private readonly IQueryRepository<RoleModule> _roleModuleQuery;
    private readonly ICommandRepository<RoleModule> _roleModuleCommand;
    private readonly IUserManagementCore _userManagementCore;
    private readonly IAuditLogQueryCore _auditLogQueryCore;
    private readonly IDashboardService _dashboardService;
    private readonly IQueryBuilder _queryBuilder;

    public PlatformService(
        IUserContext userContext,
        IPasswordHasher passwordHasher,
        IQueryRepository<PlatformTenant> tenantQuery,
        ICommandRepository<PlatformTenant> tenantCommand,
        IQueryRepository<Organization> organizationQuery,
        ICommandRepository<Organization> organizationCommand,
        IQueryRepository<User> userQuery,
        ICommandRepository<User> userCommand,
        IQueryRepository<UserOrganization> userOrganizationQuery,
        IQueryRepository<Role> roleQuery,
        ICommandRepository<Role> roleCommand,
        IQueryRepository<RoleModule> roleModuleQuery,
        ICommandRepository<RoleModule> roleModuleCommand,
        IUserManagementCore userManagementCore,
        IAuditLogQueryCore auditLogQueryCore,
        IDashboardService dashboardService,
        ILogger<PlatformService> logger,
        IQueryBuilder queryBuilder,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _passwordHasher = passwordHasher;
        _tenantQuery = tenantQuery;
        _tenantCommand = tenantCommand;
        _organizationQuery = organizationQuery;
        _organizationCommand = organizationCommand;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userOrganizationQuery = userOrganizationQuery;
        _roleQuery = roleQuery;
        _roleCommand = roleCommand;
        _roleModuleQuery = roleModuleQuery;
        _roleModuleCommand = roleModuleCommand;
        _userManagementCore = userManagementCore;
        _auditLogQueryCore = auditLogQueryCore;
        _dashboardService = dashboardService;
        _queryBuilder = queryBuilder;
    }

    public Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetDashboardAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
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
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PagedResponse<PlatformTenantDto>>(PlatformErrors.GlobalAccessRequired());

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var spec = _queryBuilder.For<PlatformTenant>()
                .Where(tenant =>
                    (string.IsNullOrWhiteSpace(search)
                     || tenant.Name.ToLower().Contains(search)
                     || tenant.Slug.ToLower().Contains(search))
                    && (!filter.StateId.HasValue || tenant.StateId == filter.StateId.Value))
                .OrderBy(query => query.OrderBy(tenant => tenant.Id))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .BuildPaged();

            var paged = await _tenantQuery.GetPagedAsync(spec, ct);
            var items = new List<PlatformTenantDto>();
            foreach (var tenant in paged.Items)
                items.Add(await MapTenantAsync(tenant, ct));

            return PagedResponseFactory.Create(new PagedList<PlatformTenantDto>(items, paged.TotalCount), page, pageSize);
        });

    public Task<Result<PlatformTenantDto>> GetTenantByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantByIdAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PlatformTenantDto>(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(id, ct);
            return tenant is null
                ? Result.Failure<PlatformTenantDto>(PlatformErrors.TenantNotFound(id))
                : Result.Success(await MapTenantAsync(tenant, ct));
        });

    public Task<Result<int>> CreateTenantAsync(PlatformTenantCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateTenantAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
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
                    MapTenantUserCreateRequest(dto.User, tenant.Id),
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
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
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

    public Task<Result> ActivateTenantAsync(int id, CancellationToken ct = default) =>
        ChangeTenantStateAsync(id, StateIdConst.ACTIVE, nameof(ActivateTenantAsync), ct);

    public Task<Result> DeactivateTenantAsync(int id, CancellationToken ct = default) =>
        ChangeTenantStateAsync(id, StateIdConst.PASSIVE, nameof(DeactivateTenantAsync), ct);

    public Task<Result<int>> CreateTenantUserAsync(int tenantId, PlatformUserCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateTenantUserAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<int>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<int>(PlatformErrors.TenantNotFound(tenantId));

            var organizationValidation = await ValidateTenantOrganizationsAsync(tenantId, dto.Organizations, ct);
            if (organizationValidation is not null)
                return Result.Failure<int>(organizationValidation);

            var userResult = await _userManagementCore.CreateUserAsync(
                MapTenantUserCreateRequest(dto, tenantId),
                UserManagementOptions.ForGlobal(),
                ct);
            return userResult.IsSuccess
                ? Result.Success(userResult.Value.UserId)
                : Result.Failure<int>(userResult.Error);
        }, ct);

    public Task<Result> UpdateTenantUserAsync(int tenantId, int userId, PlatformUserUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateTenantUserAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantUserAsync(tenantId, userId, ct) is null)
                return Result.Failure(PlatformErrors.UserNotFound(userId));

            var organizationValidation = await ValidateTenantOrganizationsAsync(tenantId, dto.Organizations, ct);
            if (organizationValidation is not null)
                return Result.Failure(organizationValidation);

            return await _userManagementCore.UpdateUserAsync(
                MapTenantUserUpdateRequest(userId, dto),
                UserManagementOptions.ForGlobal(),
                ct);
        }, ct);

    public Task<Result> BlockTenantUserAsync(int tenantId, int userId, CancellationToken ct = default) =>
        ChangeTenantUserStateAsync(tenantId, userId, StateIdConst.PASSIVE, nameof(BlockTenantUserAsync), ct);

    public Task<Result> UnblockTenantUserAsync(int tenantId, int userId, CancellationToken ct = default) =>
        ChangeTenantUserStateAsync(tenantId, userId, StateIdConst.ACTIVE, nameof(UnblockTenantUserAsync), ct);

    public Task<Result> SetTenantUserPasswordAsync(int tenantId, int userId, PlatformSetPasswordDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(SetTenantUserPasswordAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var user = await GetTenantUserAsync(tenantId, userId, ct);
            if (user is null)
                return Result.Failure(PlatformErrors.UserNotFound(userId));

            var salt = _passwordHasher.GenerateSalt();
            user.PasswordSalt = salt;
            user.PasswordHash = _passwordHasher.Hash(dto.Password, salt);
            await _userCommand.UpdateAsync(user, ct);
            return Result.Success();
        });
    public Task<Result<PagedResponse<PlatformUserDto>>> GetTenantUsersAsync(int tenantId, PlatformUserListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantUsersAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PagedResponse<PlatformUserDto>>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PagedResponse<PlatformUserDto>>(PlatformErrors.TenantNotFound(tenantId));

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var spec = _queryBuilder.For<User>()
                .Where(user =>
                    (string.IsNullOrWhiteSpace(search)
                     || user.UserName.ToLower().Contains(search)
                     || user.PhoneNumber.ToLower().Contains(search)
                     || (user.Email != null && user.Email.ToLower().Contains(search))
                     || user.FirstName.ToLower().Contains(search)
                     || user.LastName.ToLower().Contains(search))
                    && (!filter.RoleId.HasValue || user.UserOrganizations.Any(membership => membership.RoleId == filter.RoleId.Value))
                    && (!filter.UserKindId.HasValue || user.UserKindId == filter.UserKindId.Value)
                    && (!filter.StateId.HasValue || user.StateId == filter.StateId.Value)
                    && ((!filter.OrganizationId.HasValue && user.TenantId == tenantId)
                        || user.UserOrganizations.Any(membership =>
                            membership.Organization.TenantId == tenantId &&
                            (!filter.OrganizationId.HasValue || membership.OrganizationId == filter.OrganizationId.Value))))
                .As(PlatformUserDtoProjection.Summary)
                .OrderBy(query => query.OrderBy(user => user.Id))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .BuildPaged();

            var paged = await _userQuery.GetPagedAsync(spec, ct);
            return PagedResponseFactory.Create(paged, page, pageSize);
        });

    public Task<Result<PlatformUserDetailDto>> GetTenantUserByIdAsync(int tenantId, int userId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantUserByIdAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.TenantNotFound(tenantId));

            var userQuery = _queryBuilder.For<User>()
                .Where(x => x.Id == userId && x.TenantId == tenantId)
                .As(PlatformUserDtoProjection.Summary)
                .Build();
            var user = await _userQuery.GetAsync(userQuery, ct);
            if (user is null)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.UserNotFound(userId));

            var result = MapPlatformUserDetail(user);
            result.Organizations = await GetTenantUserOrganizationsAsync(tenantId, userId, null, ct);
            return result;
        });

    public Task<Result<int>> CreateTenantOrganizationAsync(int tenantId, OrganizationCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateTenantOrganizationAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<int>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<int>(PlatformErrors.TenantNotFound(tenantId));

            if (await _organizationQuery.AnyAsync(x => x.Inn == dto.Inn, ct))
                return Result.Failure<int>(PlatformErrors.OrganizationInnConflict(dto.Inn));

            var organization = new Organization
            {
                ShortName = dto.ShortName.Trim(),
                FullName = dto.FullName.Trim(),
                Inn = dto.Inn.Trim(),
                PhoneNumber = dto.PhoneNumber,
                RegionId = dto.RegionId,
                DistrictId = dto.DistrictId,
                Address = dto.Address,
                Director = dto.Director,
                IsParent = dto.IsParent,
                DefaultLanguageId = dto.DefaultLanguageId,
                TenantId = tenantId,
                SetupStatus = string.IsNullOrWhiteSpace(dto.SetupStatus) ? "pending" : dto.SetupStatus.Trim(),
                SetupCompletedAt = dto.SetupCompletedAt,
                Email = dto.Email,
                Website = dto.Website,
                Oked = dto.Oked,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };
            await _organizationCommand.CreateAsync(organization, ct);
            return Result.Success(organization.Id);
        });

    public Task<Result> ActivateTenantOrganizationAsync(int tenantId, int organizationId, CancellationToken ct = default) =>
        ChangeTenantOrganizationStateAsync(tenantId, organizationId, StateIdConst.ACTIVE, nameof(ActivateTenantOrganizationAsync), ct);

    public Task<Result> DeactivateTenantOrganizationAsync(int tenantId, int organizationId, CancellationToken ct = default) =>
        ChangeTenantOrganizationStateAsync(tenantId, organizationId, StateIdConst.PASSIVE, nameof(DeactivateTenantOrganizationAsync), ct);
    public Task<Result<PagedResponse<PlatformOrganizationDto>>> GetTenantOrganizationsAsync(int tenantId, PlatformOrganizationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantOrganizationsAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PagedResponse<PlatformOrganizationDto>>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PagedResponse<PlatformOrganizationDto>>(PlatformErrors.TenantNotFound(tenantId));

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var setupStatus = filter.SetupStatus?.Trim().ToLowerInvariant();
            var spec = _queryBuilder.For<Organization>()
                .Where(organization =>
                    organization.TenantId == tenantId
                    && (string.IsNullOrWhiteSpace(search)
                        || organization.ShortName.ToLower().Contains(search)
                        || organization.FullName.ToLower().Contains(search)
                        || organization.Inn.ToLower().Contains(search)
                        || (organization.Email != null && organization.Email.ToLower().Contains(search)))
                    && (!filter.RegionId.HasValue || organization.RegionId == filter.RegionId.Value)
                    && (!filter.StateId.HasValue || organization.StateId == filter.StateId.Value)
                    && (string.IsNullOrWhiteSpace(setupStatus) || organization.SetupStatus.ToLower() == setupStatus))
                .OrderBy(query => query.OrderBy(organization => organization.Id))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AddIncludes(builder =>
                {
                    builder.Include(x => x.Region);
                    builder.Include(x => x.District);
                    builder.Include(x => x.State);
                    builder.Include(x => x.DefaultLanguage);
                })
                .BuildPaged();

            var paged = await _organizationQuery.GetPagedAsync(spec, ct);
            var items = new List<PlatformOrganizationDto>();
            foreach (var organization in paged.Items)
                items.Add(await MapOrganizationAsync(organization, ct));

            return PagedResponseFactory.Create(new PagedList<PlatformOrganizationDto>(items, paged.TotalCount), page, pageSize);
        });

    public Task<Result<PlatformOrganizationDetailDto>> GetTenantOrganizationByIdAsync(int tenantId, int organizationId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantOrganizationByIdAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PlatformOrganizationDetailDto>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PlatformOrganizationDetailDto>(PlatformErrors.TenantNotFound(tenantId));

            var query = _queryBuilder.For<Organization>()
                .Where(x => x.Id == organizationId && x.TenantId == tenantId)
                .Build();
            query.AddIncludes(builder =>
            {
                builder.Include(x => x.Region);
                builder.Include(x => x.District);
                builder.Include(x => x.State);
                builder.Include(x => x.DefaultLanguage);
            });
            var organization = await _organizationQuery.GetAsync(query, ct);
            if (organization is null)
                return Result.Failure<PlatformOrganizationDetailDto>(PlatformErrors.OrganizationNotFound(organizationId));

            var result = MapPlatformOrganizationDetail(await MapOrganizationAsync(organization, ct));
            result.Users = await GetTenantUserOrganizationsAsync(tenantId, null, organizationId, ct);
            return result;
        });

    public Task<Result<PagedResponse<PlatformAuditLogDto>>> GetAuditLogsAsync(PlatformAuditLogListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAuditLogsAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
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

    private Task<Result> ChangeTenantStateAsync(int tenantId, short stateId, string operationName, CancellationToken ct) =>
        ExecuteAsync(operationName, async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(tenantId, ct);
            if (tenant is null)
                return Result.Failure(PlatformErrors.TenantNotFound(tenantId));

            tenant.StateId = stateId;
            tenant.UpdatedDate = DateTime.Now;
            await _tenantCommand.UpdateAsync(tenant, ct);
            return Result.Success();
        });

    private Task<Result> ChangeTenantUserStateAsync(int tenantId, int userId, short stateId, string operationName, CancellationToken ct) =>
        ExecuteAsync(operationName, async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var user = await GetTenantUserAsync(tenantId, userId, ct);
            if (user is null)
                return Result.Failure(PlatformErrors.UserNotFound(userId));

            user.StateId = stateId;
            await _userCommand.UpdateAsync(user, ct);
            return Result.Success();
        });

    private Task<Result> ChangeTenantOrganizationStateAsync(int tenantId, int organizationId, short stateId, string operationName, CancellationToken ct) =>
        ExecuteAsync(operationName, async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var organization = await GetTenantOrganizationAsync(tenantId, organizationId, ct);
            if (organization is null)
                return Result.Failure(PlatformErrors.OrganizationNotFound(organizationId));

            organization.StateId = stateId;
            await _organizationCommand.UpdateAsync(organization, ct);
            return Result.Success();
        });

    private async Task<User?> GetTenantUserAsync(int tenantId, int userId, CancellationToken ct) =>
        await _userQuery.GetAsync(_queryBuilder.For<User>()
            .Where(x => x.Id == userId && x.TenantId == tenantId)
            .Build(), ct);

    private async Task<Organization?> GetTenantOrganizationAsync(int tenantId, int organizationId, CancellationToken ct) =>
        await _organizationQuery.GetAsync(_queryBuilder.For<Organization>()
            .Where(x => x.Id == organizationId && x.TenantId == tenantId)
            .Build(), ct);
    private async Task<Error?> ValidateTenantOrganizationsAsync(int tenantId, IEnumerable<PlatformUserOrganizationCreateDto> organizations, CancellationToken ct)
    {
        var requestedOrganizationIds = organizations.Select(organization => organization.OrganizationId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (requestedOrganizationIds.Count == 0)
            return null;

        var specification = _queryBuilder.For<Organization>()
            .Where(x => requestedOrganizationIds.Contains(x.Id) && x.TenantId == tenantId)
            .As(x => x.Id)
            .Build();
        var existingOrganizationIds = (await _organizationQuery.GetAllAsync(specification, ct)).ToHashSet();
        var invalidOrganizationId = requestedOrganizationIds.FirstOrDefault(id => !existingOrganizationIds.Contains(id));

        return invalidOrganizationId == default
            ? null
            : PlatformErrors.OrganizationNotFound(invalidOrganizationId);
    }

    private Task<List<PlatformUserOrganizationDto>> GetTenantUserOrganizationsAsync(
        int tenantId,
        int? userId,
        int? organizationId,
        CancellationToken ct) =>
        _userOrganizationQuery.GetAllAsync(_queryBuilder.For<UserOrganization>()
            .Where(membership =>
                membership.User.TenantId == tenantId &&
                membership.Organization.TenantId == tenantId &&
                (!userId.HasValue || membership.UserId == userId.Value) &&
                (!organizationId.HasValue || membership.OrganizationId == organizationId.Value))
            .As(membership => new PlatformUserOrganizationDto
            {
                UserId = membership.UserId,
                UserName = membership.User.UserName,
                OrganizationId = membership.OrganizationId,
                OrganizationName = membership.Organization.ShortName,
                RoleId = membership.RoleId,
                RoleName = membership.Role == null ? null : membership.Role.FullName,
                IsDefault = membership.IsDefault,
                IsOwner = membership.IsOwner,
                StateId = membership.StateId,
                JoinedAt = membership.JoinedAt,
                InvitedByUserId = membership.InvitedByUserId,
                LastAccessAt = membership.LastAccessAt,
                BlockedAt = membership.BlockedAt
            })
            .OrderBy(query => query.OrderBy(membership => membership.OrganizationId))
            .Build(), ct);

    private async Task<PlatformTenant?> GetTenantEntityAsync(int id, CancellationToken ct) =>
        await _tenantQuery.GetAsync(_queryBuilder.For<PlatformTenant>().Where(x => x.Id == id).Build(), ct);

    private async Task<PlatformTenantDto> MapTenantAsync(PlatformTenant tenant, CancellationToken ct)
    {
        var ownerUserName = tenant.OwnerUserId.HasValue
            ? await _userQuery.GetAsync(_queryBuilder.For<User>()
                .Where(x => x.Id == tenant.OwnerUserId.Value)
                .As(x => x.UserName)
                .Build(), ct)
            : null;

        var organizationIds = await _organizationQuery.GetAllAsync(_queryBuilder.For<Organization>()
            .Where(x => x.TenantId == tenant.Id)
            .As(x => x.Id)
            .Build(), ct);
        var membershipUserIds = organizationIds.Count == 0
            ? []
            : await _userOrganizationQuery.GetAllAsync(_queryBuilder.For<UserOrganization>()
                .Where(x => organizationIds.Contains(x.OrganizationId) && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.UserId)
                .Build(), ct);
        var tenantUserIds = await _userQuery.GetAllAsync(_queryBuilder.For<User>()
            .Where(x => x.TenantId == tenant.Id)
            .As(x => x.Id)
            .Build(), ct);

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

    private static PlatformUserDetailDto MapPlatformUserDetail(PlatformUserDto user) =>
        new()
        {
            Id = user.Id,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = user.TenantId,
            UserKindId = user.UserKindId,
            EmailVerified = user.EmailVerified,
            EmailVerifiedAt = user.EmailVerifiedAt,
            LastLoginIp = user.LastLoginIp,
            Timezone = user.Timezone,
            LastAccessTime = user.LastAccessTime,
            UserKindCode = user.UserKindCode,
            StateId = user.StateId,
            StateName = user.StateName,
            CreatedDate = user.CreatedDate,
            OrganizationsCount = user.OrganizationsCount
        };

    private static PlatformOrganizationDetailDto MapPlatformOrganizationDetail(PlatformOrganizationDto organization) =>
        new()
        {
            Id = organization.Id,
            ShortName = organization.ShortName,
            FullName = organization.FullName,
            Inn = organization.Inn,
            PhoneNumber = organization.PhoneNumber,
            RegionId = organization.RegionId,
            RegionName = organization.RegionName,
            DistrictId = organization.DistrictId,
            DistrictName = organization.DistrictName,
            Address = organization.Address,
            Director = organization.Director,
            IsParent = organization.IsParent,
            StateId = organization.StateId,
            StateName = organization.StateName,
            DefaultLanguageId = organization.DefaultLanguageId,
            DefaultLanguageName = organization.DefaultLanguageName,
            TenantId = organization.TenantId,
            TenantName = organization.TenantName,
            SetupStatus = organization.SetupStatus,
            SetupCompletedAt = organization.SetupCompletedAt,
            Email = organization.Email,
            Website = organization.Website,
            Oked = organization.Oked,
            CreatedDate = organization.CreatedDate,
            UsersCount = organization.UsersCount
        };

    private static UserManagementCreateRequest MapTenantUserCreateRequest(PlatformUserCreateDto dto, int tenantId) =>
        new()
        {
            TenantId = tenantId,
            UserName = dto.UserName,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserKindId = dto.UserKindId,
            LanguageId = dto.LanguageId,
            EmailVerified = dto.EmailVerified,
            Timezone = dto.Timezone,
            Organizations = MapMemberships(dto.Organizations)
        };

    private static UserManagementUpdateRequest MapTenantUserUpdateRequest(int userId, PlatformUserUpdateDto dto) =>
        new()
        {
            UserId = userId,
            UserName = dto.UserName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserKindId = dto.UserKindId,
            LanguageId = dto.LanguageId,
            EmailVerified = dto.EmailVerified,
            Timezone = dto.Timezone,
            StateId = dto.StateId,
            Organizations = MapMemberships(dto.Organizations)
        };

    private static List<UserManagementMembershipRequest> MapMemberships(
        IEnumerable<PlatformUserOrganizationCreateDto> organizations) =>
        organizations.Select(organization => new UserManagementMembershipRequest
        {
            OrganizationId = organization.OrganizationId,
            RoleId = organization.RoleId,
            IsDefault = organization.IsDefault,
            IsOwner = organization.IsOwner,
            InvitedByUserId = organization.InvitedByUserId
        }).ToList();
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
        var page = await _userOrganizationQuery.GetPagedAsync(_queryBuilder.For<UserOrganization>()
            .Where(criteria)
            .Take(1)
            .BuildPaged(), ct);
        return page.TotalCount;
    }
}

using System.Text.RegularExpressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
using Application.Features.AuditLogs;
using Application.Features.Organizations;
using Application.Features.OrganizationSetup;
using Application.Features.Users.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.Platform;

public sealed class PlatformService : BaseService, IPlatformService
{
    private static readonly HashSet<string> InventoryValuationMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "fifo",
        "lifo",
        "average"
    };

    private readonly IUserContext _userContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<PlatformTenant> _tenantQuery;
    private readonly ICommandRepository<PlatformTenant> _tenantCommand;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly ICommandRepository<Organization> _organizationCommand;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly ICommandRepository<UserOrganization> _userOrganizationCommand;
    private readonly IUserManagementCore _userManagementCore;
    private readonly IOrganizationManagementCore _organizationManagementCore;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly IQueryRepository<TaxType> _taxTypeQuery;
    private readonly IQueryRepository<AccountingPolicy> _accountingPolicyQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<OrganizationSetupState> _setupStateQuery;
    private readonly IAuditLogQueryCore _auditLogQueryCore;
    private readonly IOrganizationSetupCore _organizationSetupCore;
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
        ICommandRepository<UserOrganization> userOrganizationCommand,
        IUserManagementCore userManagementCore,
        IOrganizationManagementCore organizationManagementCore,
        IQueryRepository<Role> roleQuery,
        IQueryRepository<TaxType> taxTypeQuery,
        IQueryRepository<AccountingPolicy> accountingPolicyQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<OrganizationSetupState> setupStateQuery,
        IAuditLogQueryCore auditLogQueryCore,
        IOrganizationSetupCore organizationSetupCore,
        IDashboardService dashboardService,
        ILogger<PlatformService> logger,
        IQueryBuilder queryBuilder,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
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
        _userOrganizationCommand = userOrganizationCommand;
        _userManagementCore = userManagementCore;
        _organizationManagementCore = organizationManagementCore;
        _roleQuery = roleQuery;
        _taxTypeQuery = taxTypeQuery;
        _accountingPolicyQuery = accountingPolicyQuery;
        _currencyQuery = currencyQuery;
        _setupStateQuery = setupStateQuery;
        _auditLogQueryCore = auditLogQueryCore;
        _organizationSetupCore = organizationSetupCore;
        _dashboardService = dashboardService;
        _queryBuilder = queryBuilder;
    }

    public Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetDashboardAsync), async () =>
        {
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

    public Task<Result<PlatformTenantDetailDto>> GetTenantByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetTenantByIdAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformTenantDetailDto>(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(id, ct);
            if (tenant is null)
                return Result.Failure<PlatformTenantDetailDto>(PlatformErrors.TenantNotFound(id));

            var dto = await MapTenantDetailAsync(tenant, ct);
            return dto;
        });

    public Task<Result<int>> CreateTenantAsync(PlatformTenantCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateTenantAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<int>(PlatformErrors.GlobalAccessRequired());

            var slug = NormalizeSlug(dto.Slug ?? dto.Name);
            var slugExists = await _tenantQuery.AnyAsync(x => x.Slug == slug, ct);
            if (slugExists)
                return Result.Failure<int>(PlatformErrors.TenantSlugConflict(slug));

            if (dto.OwnerUserId.HasValue)
            {
                var ownerExists = await _userQuery.AnyAsync(x => x.Id == dto.OwnerUserId.Value, ct);
                if (!ownerExists)
                    return Result.Failure<int>(PlatformErrors.UserNotFound(dto.OwnerUserId.Value));
            }

            var tenant = new PlatformTenant
            {
                Name = dto.Name.Trim(),
                Slug = slug,
                OwnerUserId = dto.OwnerUserId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _tenantCommand.CreateAsync(tenant, ct);
            return tenant.Id;
        });

    public Task<Result> UpdateTenantAsync(int id, PlatformTenantUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateTenantAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(id, ct);
            if (tenant is null)
                return Result.Failure(PlatformErrors.TenantNotFound(id));

            var slug = NormalizeSlug(dto.Slug ?? dto.Name);
            var slugExists = await _tenantQuery.AnyAsync(x => x.Id != id && x.Slug == slug, ct);
            if (slugExists)
                return Result.Failure(PlatformErrors.TenantSlugConflict(slug));

            if (dto.OwnerUserId.HasValue)
            {
                var ownerExists = await _userQuery.AnyAsync(x => x.Id == dto.OwnerUserId.Value, ct);
                if (!ownerExists)
                    return Result.Failure(PlatformErrors.UserNotFound(dto.OwnerUserId.Value));
            }

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

    public Task<Result<PagedResponse<PlatformUserDto>>> GetUsersAsync(PlatformUserListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetUsersAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PagedResponse<PlatformUserDto>>(PlatformErrors.GlobalAccessRequired());

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
                    && (!filter.OrganizationId.HasValue || user.UserOrganizations.Any(uo => uo.OrganizationId == filter.OrganizationId.Value))
                    && (!filter.TenantId.HasValue || user.UserOrganizations.Any(uo => uo.Organization.TenantId == filter.TenantId.Value)),
                OrderBy = query => query.OrderBy(user => user.Id),
                Skip = (page - 1) * pageSize,
                Take = pageSize,
                Selector = PlatformUserDtoProjection.Summary
            };

            var paged = await _userQuery.GetPagedAsync(spec, ct);
            return PagedResponseFactory.Create(paged, page, pageSize);
        });

    public Task<Result<PlatformUserDetailDto>> GetUserByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetUserByIdAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.GlobalAccessRequired());

            var user = await _userQuery.GetAsync(new QuerySpecification<User, PlatformUserDto>
            {
                Criteria = x => x.Id == id,
                Selector = PlatformUserDtoProjection.Summary
            }, ct);

            if (user is null)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.UserNotFound(id));

            return CreatePlatformUserDetailDto(user, await GetUserMembershipDtosAsync(id, ct));
        });

    public Task<Result<int>> CreateUserAsync(PlatformUserCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateUserAsync), async () =>
        {
            var coreResult = await _userManagementCore.CreateUserAsync(
                MapUserCreateRequest(dto),
                UserManagementOptions.ForGlobal(),
                ct);

            return coreResult.IsSuccess
                ? coreResult.Value.UserId
                : Result.Failure<int>(coreResult.Error);
        }, ct);

    public Task<Result> UpdateUserAsync(int id, PlatformUserUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateUserAsync), () =>
            _userManagementCore.UpdateUserAsync(
                MapUserUpdateRequest(id, dto),
                UserManagementOptions.ForGlobal(),
                ct), ct);

    public Task<Result> BlockUserAsync(int id, CancellationToken ct = default) =>
        ChangeUserStateAsync(id, StateIdConst.PASSIVE, nameof(BlockUserAsync), ct);

    public Task<Result> UnblockUserAsync(int id, CancellationToken ct = default) =>
        ChangeUserStateAsync(id, StateIdConst.ACTIVE, nameof(UnblockUserAsync), ct);

    public Task<Result<PagedResponse<PlatformOrganizationDto>>> GetOrganizationsAsync(PlatformOrganizationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetOrganizationsAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PagedResponse<PlatformOrganizationDto>>(PlatformErrors.GlobalAccessRequired());

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var setupStatus = filter.SetupStatus?.Trim().ToLowerInvariant();

            var spec = new PagedQuerySpecification<Organization>
            {
                Criteria = organization =>
                    (string.IsNullOrWhiteSpace(search)
                     || organization.ShortName.ToLower().Contains(search)
                     || organization.FullName.ToLower().Contains(search)
                     || organization.Inn.ToLower().Contains(search)
                     || (organization.Email != null && organization.Email.ToLower().Contains(search)))
                    && (!filter.TenantId.HasValue || organization.TenantId == filter.TenantId.Value)
                    && (!filter.RegionId.HasValue || organization.RegionId == filter.RegionId.Value)
                    && (!filter.StateId.HasValue || organization.StateId == filter.StateId.Value)
                    && (string.IsNullOrWhiteSpace(setupStatus) || organization.SetupStatus.ToLower() == setupStatus),
                OrderBy = query => query.OrderBy(organization => organization.Id),
                Skip = (page - 1) * pageSize,
                Take = pageSize
            };
            spec.AddIncludes(b =>
            {
                b.Include(x => x.Region);
                b.Include(x => x.District);
                b.Include(x => x.State);
                b.Include(x => x.DefaultLanguage);
            });

            var paged = await _organizationQuery.GetPagedAsync(spec, ct);
            var items = new List<PlatformOrganizationDto>();
            foreach (var organization in paged.Items)
                items.Add(await MapOrganizationAsync(organization, ct));

            return PagedResponseFactory.Create(new PagedList<PlatformOrganizationDto>(items, paged.TotalCount), page, pageSize);
        });

    public Task<Result<PlatformOrganizationDetailDto>> GetOrganizationByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetOrganizationByIdAsync), async () =>
        {
            var organizationResult = await _organizationManagementCore.GetOrganizationAsync(
                id,
                OrganizationManagementOptions.ForGlobal(includeDetails: true),
                ct);
            if (!organizationResult.IsSuccess)
                return Result.Failure<PlatformOrganizationDetailDto>(organizationResult.Error);

            var baseDto = await MapOrganizationAsync(organizationResult.Value, ct);
            var setup = await _setupStateQuery.GetAsync(new QuerySpecification<OrganizationSetupState>
            {
                Criteria = x => x.OrganizationId == id
            }, ct);

            return CreatePlatformOrganizationDetailDto(
                baseDto,
                await GetOrganizationMembershipDtosAsync(id, ct),
                setup is null ? null : MapSetup(setup));
        });

    public Task<Result> UpdateOrganizationAsync(int id, PlatformOrganizationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateOrganizationAsync), () =>
            _organizationManagementCore.UpdateOrganizationAsync(
                MapOrganizationUpdateRequest(id, dto),
                OrganizationManagementOptions.ForGlobal(),
                ct));

    public Task<Result> ActivateOrganizationAsync(int id, CancellationToken ct = default) =>
        ChangeOrganizationStateAsync(id, StateIdConst.ACTIVE, setupStatus: null, nameof(ActivateOrganizationAsync), ct);

    public Task<Result> DeactivateOrganizationAsync(int id, CancellationToken ct = default) =>
        ChangeOrganizationStateAsync(id, StateIdConst.PASSIVE, setupStatus: null, nameof(DeactivateOrganizationAsync), ct);

    public Task<Result> ArchiveOrganizationAsync(int id, CancellationToken ct = default) =>
        ChangeOrganizationStateAsync(id, StateIdConst.PASSIVE, setupStatus: "archived", nameof(ArchiveOrganizationAsync), ct);

    public Task<Result<AccountantWorkspaceDto>> CreateAccountantWorkspaceAsync(AccountantWorkspaceCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAccountantWorkspaceAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.GlobalAccessRequired());

            var slug = NormalizeSlug(dto.TenantSlug ?? dto.TenantName);
            var slugExists = await _tenantQuery.AnyAsync(x => x.Slug == slug, ct);
            if (slugExists)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.TenantSlugConflict(slug));

            var innExists = await _organizationQuery.AnyAsync(x => x.Inn == dto.Inn, ct);
            if (innExists)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.OrganizationInnConflict(dto.Inn));

            var accountingValidation = await ValidateAccountingReferencesAsync(dto.AccountingPolicyId, dto.BaseCurrencyId, ct);
            if (accountingValidation is not null)
                return Result.Failure<AccountantWorkspaceDto>(accountingValidation);

            if (dto.TaxTypeId.HasValue)
            {
                var taxTypeExists = await _taxTypeQuery.AnyAsync(x => x.Id == dto.TaxTypeId.Value && x.StateId == StateIdConst.ACTIVE, ct);
                if (!taxTypeExists)
                    return Result.Failure<AccountantWorkspaceDto>(Application.Features.OrganizationSetup.OrganizationSetupErrors.TaxTypeNotFound(dto.TaxTypeId.Value));
            }

            var hasTax = dto.TaxTypeId.HasValue;
            var hasAccounting = HasAccountingSettings(dto) || dto.CompleteSetup;
            var hasDefaults = HasDefaults(dto.Defaults);
            if (dto.CompleteSetup)
            {
                if (!hasTax)
                    return Result.Failure<AccountantWorkspaceDto>(Application.Features.OrganizationSetup.OrganizationSetupErrors.SetupNotReady("tax-settings"));
                if (!hasAccounting)
                    return Result.Failure<AccountantWorkspaceDto>(Application.Features.OrganizationSetup.OrganizationSetupErrors.SetupNotReady("accounting-policy"));
                if (!hasDefaults)
                    return Result.Failure<AccountantWorkspaceDto>(Application.Features.OrganizationSetup.OrganizationSetupErrors.SetupNotReady("defaults"));
            }

            var now = DateTime.Now;
            var tenant = new PlatformTenant
            {
                Name = dto.TenantName.Trim(),
                Slug = slug,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now
            };
            await _tenantCommand.CreateAsync(tenant, ct);

            var isCompleted = dto.CompleteSetup;
            var organization = new Organization
            {
                ShortName = dto.OrganizationShortName.Trim(),
                FullName = dto.OrganizationFullName.Trim(),
                Inn = dto.Inn.Trim(),
                PhoneNumber = dto.OrganizationPhoneNumber,
                RegionId = dto.RegionId,
                DistrictId = dto.DistrictId,
                Address = dto.Address,
                Director = dto.Director,
                IsParent = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                DefaultLanguageId = dto.DefaultLanguageId,
                TenantId = tenant.Id,
                SetupStatus = isCompleted ? "completed" : "pending",
                SetupCompletedAt = isCompleted ? now : null,
                Email = dto.OrganizationEmail,
                Website = dto.Website,
                Oked = dto.Oked
            };
            await _organizationCommand.CreateAsync(organization, ct);

            var userResult = await _userManagementCore.CreateUserAsync(
                MapWorkspaceOwnerCreateRequest(dto, organization.Id),
                UserManagementOptions.ForGlobal(),
                ct);
            if (!userResult.IsSuccess)
                return Result.Failure<AccountantWorkspaceDto>(userResult.Error);

            tenant.OwnerUserId = userResult.Value.UserId;
            tenant.UpdatedDate = now;
            await _tenantCommand.UpdateAsync(tenant, ct);

            if (hasAccounting)
            {
                var method = NormalizeInventoryValuationMethod(dto.InventoryValuationMethod ?? "fifo");
                if (!InventoryValuationMethods.Contains(method))
                    return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.InvalidInventoryValuationMethod(method));
            }

            await _organizationSetupCore.SeedWorkspaceSetupAsync(
                new WorkspaceSetupInitializationRequest
                {
                    OrganizationId = organization.Id,
                    HasTax = hasTax,
                    TaxSettings = hasTax && dto.TaxTypeId.HasValue
                        ? new OrganizationSetupTaxSettingsWriteModel
                        {
                            TaxTypeId = dto.TaxTypeId.Value,
                            IsVatPayer = dto.IsVatPayer,
                            VatRegistrationNumber = dto.VatRegistrationNumber,
                            EffectiveFrom = dto.TaxEffectiveFrom ?? DateOnly.FromDateTime(now),
                            EffectiveTo = dto.TaxEffectiveTo,
                            StateId = StateIdConst.ACTIVE,
                            CreatedDate = now
                        }
                        : null,
                    HasAccounting = hasAccounting,
                    AccountingPolicy = hasAccounting
                        ? new OrganizationSetupAccountingPolicyWriteModel
                        {
                            InventoryValuationMethod = NormalizeInventoryValuationMethod(dto.InventoryValuationMethod ?? "fifo"),
                            AccountingPolicyId = dto.AccountingPolicyId,
                            BaseCurrencyId = dto.BaseCurrencyId,
                            AccountingStartDate = dto.AccountingStartDate,
                            FiscalYearStartMonth = dto.FiscalYearStartMonth
                        }
                        : null,
                    HasDefaults = hasDefaults,
                    Defaults = hasDefaults && dto.Defaults is not null
                        ? new OrganizationSetupDefaultsWriteModel
                        {
                            BranchId = dto.Defaults.BranchId,
                            WarehouseId = dto.Defaults.WarehouseId,
                            CashBoxId = dto.Defaults.CashBoxId,
                            BankAccountId = dto.Defaults.BankAccountId,
                            ReceivableAccountId = dto.Defaults.ReceivableAccountId,
                            PayableAccountId = dto.Defaults.PayableAccountId,
                            InventoryAccountId = dto.Defaults.InventoryAccountId,
                            CashAccountId = dto.Defaults.CashAccountId,
                            BankAccountingAccountId = dto.Defaults.BankAccountingAccountId,
                            RevenueAccountId = dto.Defaults.RevenueAccountId,
                            ExpenseAccountId = dto.Defaults.ExpenseAccountId,
                            CogsAccountId = dto.Defaults.CogsAccountId,
                            CreatedDate = now
                        }
                        : null,
                    UsersCompleted = true,
                    IsCompleted = isCompleted,
                    Timestamp = now
                },
                ct);

            var workspace = await BuildWorkspaceDtoAsync(organization.Id, ct);
            return workspace!;
        }, ct);

    public Task<Result<AccountantWorkspaceDto>> GetAccountantWorkspaceAsync(int organizationId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAccountantWorkspaceAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.GlobalAccessRequired());

            var workspace = await BuildWorkspaceDtoAsync(organizationId, ct);
            if (workspace is null)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.OrganizationNotFound(organizationId));

            return workspace;
        });

    public Task<Result<PlatformUserOrganizationDto>> AttachUserToOrganizationAsync(int userId, PlatformUserOrganizationCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(AttachUserToOrganizationAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformUserOrganizationDto>(PlatformErrors.GlobalAccessRequired());

            var validation = await ValidateUserOrganizationReferencesAsync(userId, dto.OrganizationId, dto.RoleId, ct);
            if (validation is not null)
                return Result.Failure<PlatformUserOrganizationDto>(validation);

            var membership = await GetMembershipAsync(userId, dto.OrganizationId, ct);
            if (membership is not null && membership.StateId == StateIdConst.ACTIVE)
                return Result.Failure<PlatformUserOrganizationDto>(PlatformErrors.UserOrganizationConflict(userId, dto.OrganizationId));

            if (dto.IsDefault)
                await ClearUserDefaultOrganizationsAsync(userId, ct);

            var now = DateTime.Now;
            if (membership is null)
            {
                membership = new UserOrganization
                {
                    UserId = userId,
                    OrganizationId = dto.OrganizationId,
                    CreatedDate = now,
                    JoinedAt = now
                };
                ApplyMembershipCreateDto(membership, dto, now);
                await _userOrganizationCommand.CreateAsync(membership, ct);
            }
            else
            {
                ApplyMembershipCreateDto(membership, dto, now);
                await _userOrganizationCommand.UpdateAsync(membership, ct);
            }

            return MapMembership(membership);
        }, ct);

    public Task<Result> UpdateUserOrganizationAsync(int userId, int organizationId, PlatformUserOrganizationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateUserOrganizationAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var validation = await ValidateUserOrganizationReferencesAsync(userId, organizationId, dto.RoleId, ct);
            if (validation is not null)
                return Result.Failure(validation);

            var membership = await GetMembershipAsync(userId, organizationId, ct);
            if (membership is null)
                return Result.Failure(PlatformErrors.UserOrganizationNotFound(userId, organizationId));

            if (dto.IsDefault == true)
                await ClearUserDefaultOrganizationsAsync(userId, ct);

            if (dto.RoleId.HasValue)
                membership.RoleId = dto.RoleId;
            if (dto.IsDefault.HasValue)
                membership.IsDefault = dto.IsDefault.Value;
            if (dto.IsOwner.HasValue)
                membership.IsOwner = dto.IsOwner.Value;
            if (dto.StateId.HasValue)
                membership.StateId = dto.StateId.Value;
            if (dto.IsBlocked.HasValue)
            {
                membership.BlockedAt = dto.IsBlocked.Value ? DateTime.Now : null;
                membership.StateId = dto.IsBlocked.Value ? StateIdConst.PASSIVE : StateIdConst.ACTIVE;
            }

            await _userOrganizationCommand.UpdateAsync(membership, ct);
            return Result.Success();
        }, ct);

    public Task<Result> RemoveUserFromOrganizationAsync(int userId, int organizationId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(RemoveUserFromOrganizationAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var membership = await GetMembershipAsync(userId, organizationId, ct);
            if (membership is null)
                return Result.Failure(PlatformErrors.UserOrganizationNotFound(userId, organizationId));

            membership.StateId = StateIdConst.PASSIVE;
            membership.IsDefault = false;
            membership.BlockedAt = DateTime.Now;
            await _userOrganizationCommand.UpdateAsync(membership, ct);
            return Result.Success();
        });

    public Task<Result> SetUserPasswordAsync(int userId, PlatformSetPasswordDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(SetUserPasswordAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var user = await _userQuery.GetAsync(new QuerySpecification<User> { Criteria = x => x.Id == userId }, ct);
            if (user is null)
                return Result.Failure(PlatformErrors.UserNotFound(userId));

            var salt = _passwordHasher.GenerateSalt();
            user.PasswordSalt = salt;
            user.PasswordHash = _passwordHasher.Hash(dto.Password, salt);

            await _userCommand.UpdateAsync(user, ct);
            return Result.Success();
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

    private static UserManagementCreateRequest MapUserCreateRequest(PlatformUserCreateDto dto) =>
        new()
        {
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
            Timezone = dto.Timezone,
            Organizations = dto.Organizations
                .Select(item => new UserManagementMembershipRequest
                {
                    OrganizationId = item.OrganizationId,
                    RoleId = item.RoleId,
                    IsDefault = item.IsDefault,
                    IsOwner = item.IsOwner,
                    InvitedByUserId = item.InvitedByUserId
                })
                .ToList()
        };

    private static UserManagementUpdateRequest MapUserUpdateRequest(int id, PlatformUserUpdateDto dto) =>
        new()
        {
            UserId = id,
            UserName = dto.UserName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
            LanguageId = dto.LanguageId,
            EmailVerified = dto.EmailVerified,
            IsPlatformAdmin = dto.IsPlatformAdmin,
            Timezone = dto.Timezone,
            StateId = dto.StateId,
            Organizations = dto.Organizations?
                .Select(item => new UserManagementMembershipRequest
                {
                    OrganizationId = item.OrganizationId,
                    RoleId = item.RoleId,
                    IsDefault = item.IsDefault,
                    IsOwner = item.IsOwner,
                    InvitedByUserId = item.InvitedByUserId
                })
                .ToList()
        };

    private static UserManagementCreateRequest MapWorkspaceOwnerCreateRequest(AccountantWorkspaceCreateDto dto, int organizationId) =>
        new()
        {
            UserName = dto.UserName,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
            LanguageId = dto.LanguageId,
            EmailVerified = !string.IsNullOrWhiteSpace(dto.Email),
            IsPlatformAdmin = false,
            Timezone = dto.Timezone,
            Organizations =
            [
                new UserManagementMembershipRequest
                {
                    OrganizationId = organizationId,
                    RoleId = dto.RoleId,
                    IsDefault = true,
                    IsOwner = true
                }
            ]
        };

    private static OrganizationManagementUpdateRequest MapOrganizationUpdateRequest(int id, PlatformOrganizationUpdateDto dto) =>
        new()
        {
            OrganizationId = id,
            ShortName = dto.ShortName,
            FullName = dto.FullName,
            Inn = dto.Inn,
            PhoneNumber = dto.PhoneNumber,
            RegionId = dto.RegionId,
            DistrictId = dto.DistrictId,
            Address = dto.Address,
            Director = dto.Director,
            IsParent = dto.IsParent,
            DefaultLanguageId = dto.DefaultLanguageId,
            TenantId = dto.TenantId,
            SetupStatus = dto.SetupStatus,
            SetupCompletedAt = dto.SetupCompletedAt,
            Email = dto.Email,
            Website = dto.Website,
            Oked = dto.Oked,
            StateId = dto.StateId
        };

    private Task<Result> ChangeUserStateAsync(int id, short stateId, string operationName, CancellationToken ct) =>
        ExecuteAsync(operationName, async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var user = await _userQuery.GetAsync(new QuerySpecification<User> { Criteria = x => x.Id == id }, ct);
            if (user is null)
                return Result.Failure(PlatformErrors.UserNotFound(id));

            user.StateId = stateId;
            await _userCommand.UpdateAsync(user, ct);
            return Result.Success();
        });

    private Task<Result> ChangeOrganizationStateAsync(int id, short stateId, string? setupStatus, string operationName, CancellationToken ct) =>
        ExecuteAsync(operationName, async () =>
        {
            var organizationResult = await _organizationManagementCore.GetOrganizationAsync(
                id,
                OrganizationManagementOptions.ForGlobal(),
                ct);
            if (!organizationResult.IsSuccess)
                return Result.Failure(organizationResult.Error);

            var organization = organizationResult.Value;

            organization.StateId = stateId;
            if (!string.IsNullOrWhiteSpace(setupStatus))
            {
                organization.SetupStatus = setupStatus;
            }
            else if (stateId == StateIdConst.ACTIVE && string.Equals(organization.SetupStatus, "archived", StringComparison.OrdinalIgnoreCase))
            {
                var setup = await _setupStateQuery.GetAsync(new QuerySpecification<OrganizationSetupState>
                {
                    Criteria = x => x.OrganizationId == organization.Id
                }, ct);
                organization.SetupStatus = setup?.IsCompleted == true ? "completed" : "pending";
            }

            await _organizationCommand.UpdateAsync(organization, ct);
            return Result.Success();
        });

    private Task<Result> ChangeTenantStateAsync(int id, short stateId, string operationName, CancellationToken ct) =>
        ExecuteAsync(operationName, async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var tenant = await GetTenantEntityAsync(id, ct);
            if (tenant is null)
                return Result.Failure(PlatformErrors.TenantNotFound(id));

            tenant.StateId = stateId;
            tenant.UpdatedDate = DateTime.Now;
            await _tenantCommand.UpdateAsync(tenant, ct);
            return Result.Success();
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

        var organizationsCount = await CountOrganizationsAsync(x => x.TenantId == tenant.Id, ct);
        var orgIds = await _organizationQuery.GetAllAsync(new QuerySpecification<Organization, int>
        {
            Criteria = x => x.TenantId == tenant.Id,
            Selector = x => x.Id
        }, ct);

        var usersCount = orgIds.Count == 0
            ? 0
            : await CountUserOrganizationsAsync(x => orgIds.Contains(x.OrganizationId), ct);

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
            OrganizationsCount = organizationsCount,
            UsersCount = usersCount
        };
    }

    private async Task<PlatformTenantDetailDto> MapTenantDetailAsync(PlatformTenant tenant, CancellationToken ct)
    {
        var baseDto = await MapTenantAsync(tenant, ct);
        var organizations = await _organizationQuery.GetAllAsync(new QuerySpecification<Organization, PlatformOrganizationItemDto>
        {
            Criteria = x => x.TenantId == tenant.Id,
            OrderBy = query => query.OrderBy(x => x.Id),
            Selector = x => new PlatformOrganizationItemDto
            {
                Id = x.Id,
                ShortName = x.ShortName,
                FullName = x.FullName,
                Inn = x.Inn,
                SetupStatus = x.SetupStatus,
                StateId = x.StateId,
                CreatedDate = x.CreatedDate
            }
        }, ct);

        return new PlatformTenantDetailDto
        {
            Id = baseDto.Id,
            Name = baseDto.Name,
            Slug = baseDto.Slug,
            OwnerUserId = baseDto.OwnerUserId,
            OwnerUserName = baseDto.OwnerUserName,
            StateId = baseDto.StateId,
            CreatedDate = baseDto.CreatedDate,
            UpdatedDate = baseDto.UpdatedDate,
            OrganizationsCount = baseDto.OrganizationsCount,
            UsersCount = baseDto.UsersCount,
            Organizations = organizations
        };
    }

    private async Task<PlatformOrganizationDto> MapOrganizationAsync(Organization organization, CancellationToken ct)
    {
        var tenantQuery = _queryBuilder.For<PlatformTenant>()
                                        .Where(x => x.Id == organization.TenantId)
                                        .As(s => s.Name)
                                        .Build();

        var tenantName = await _tenantQuery.GetAsync(tenantQuery, ct);

        var usersCount = await CountUserOrganizationsAsync(x => x.OrganizationId == organization.Id && x.StateId == StateIdConst.ACTIVE, ct);

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

    private async Task<AccountantWorkspaceDto?> BuildWorkspaceDtoAsync(int organizationId, CancellationToken ct)
    {
        var organizationQuery = _queryBuilder.For<Organization>()
                                        .Where(x => x.Id == organizationId)
                                        .Build();

        var organization = await _organizationQuery.GetAsync(organizationQuery, ct);

        if (organization is null)
            return null;

        var tenant = await GetTenantEntityAsync(organization.TenantId, ct);
        if (tenant is null)
            return null;

        var membership = await _userOrganizationQuery.GetAsync(new QuerySpecification<UserOrganization>
        {
            Criteria = x => x.OrganizationId == organization.Id && x.IsOwner && x.StateId == StateIdConst.ACTIVE
        }, ct)
        ?? await _userOrganizationQuery.GetAsync(new QuerySpecification<UserOrganization>
        {
            Criteria = x => x.OrganizationId == organization.Id && x.StateId == StateIdConst.ACTIVE
        }, ct);

        if (membership is null)
            return null;

        var user = await _userQuery.GetAsync(new QuerySpecification<User> { Criteria = x => x.Id == membership.UserId }, ct);
        if (user is null)
            return null;

        var setup = await _setupStateQuery.GetAsync(new QuerySpecification<OrganizationSetupState>
        {
            Criteria = x => x.OrganizationId == organization.Id
        }, ct);

        return new AccountantWorkspaceDto
        {
            Tenant = await MapTenantAsync(tenant, ct),
            Organization = new PlatformOrganizationItemDto
            {
                Id = organization.Id,
                ShortName = organization.ShortName,
                FullName = organization.FullName,
                Inn = organization.Inn,
                SetupStatus = organization.SetupStatus,
                StateId = organization.StateId,
                CreatedDate = organization.CreatedDate
            },
            User = CreateWorkspaceUserDto(user),
            Membership = MapMembership(membership),
            Setup = setup is null
                ? new PlatformWorkspaceSetupDto { CurrentStep = "organization" }
                : MapSetup(setup)
        };
    }

    private static PlatformWorkspaceSetupDto MapSetup(OrganizationSetupState setup) =>
        new()
        {
            CurrentStep = setup.CurrentStep,
            OrganizationCompleted = setup.OrganizationCompleted,
            TaxCompleted = setup.TaxCompleted,
            AccountingCompleted = setup.AccountingCompleted,
            DefaultsCompleted = setup.DefaultsCompleted,
            UsersCompleted = setup.UsersCompleted,
            IsCompleted = setup.IsCompleted,
            CompletedAt = setup.CompletedAt
        };

    private async Task<Error?> ValidateUserOrganizationReferencesAsync(int userId, int organizationId, int? roleId, CancellationToken ct)
    {
        var userExists = await _userQuery.AnyAsync(x => x.Id == userId, ct);
        if (!userExists)
            return PlatformErrors.UserNotFound(userId);

        var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == organizationId, ct);
        if (!organizationExists)
            return PlatformErrors.OrganizationNotFound(organizationId);

        if (roleId.HasValue)
        {
            var roleExists = await _roleQuery.AnyAsync(x => x.Id == roleId.Value && x.StateId == StateIdConst.ACTIVE, ct);
            if (!roleExists)
                return PlatformErrors.RoleNotFound(roleId.Value);
        }

        return null;
    }

    private async Task<Error?> ValidateAccountingReferencesAsync(short? accountingPolicyId, short? baseCurrencyId, CancellationToken ct)
    {
        if (accountingPolicyId.HasValue)
        {
            var exists = await _accountingPolicyQuery.AnyAsync(x => x.Id == accountingPolicyId.Value && x.StateId == StateIdConst.ACTIVE, ct);
            if (!exists)
                return Application.Features.OrganizationSetup.OrganizationSetupErrors.AccountingPolicyNotFound(accountingPolicyId.Value);
        }

        if (baseCurrencyId.HasValue)
        {
            var exists = await _currencyQuery.AnyAsync(x => x.Id == baseCurrencyId.Value && x.StateId == StateIdConst.ACTIVE, ct);
            if (!exists)
                return Application.Features.OrganizationSetup.OrganizationSetupErrors.CurrencyNotFound(baseCurrencyId.Value);
        }

        return null;
    }

    private async Task<UserOrganization?> GetMembershipAsync(int userId, int organizationId, CancellationToken ct) =>
        await _userOrganizationQuery.GetAsync(new QuerySpecification<UserOrganization>
        {
            Criteria = x => x.UserId == userId && x.OrganizationId == organizationId
        }, ct);

    private async Task<List<PlatformUserOrganizationDto>> GetUserMembershipDtosAsync(int userId, CancellationToken ct) =>
        await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization, PlatformUserOrganizationDto>
        {
            Criteria = x => x.UserId == userId,
            OrderBy = query => query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.OrganizationName),
            Selector = x => new PlatformUserOrganizationDto
            {
                UserId = x.UserId,
                UserName = x.User.UserName,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.Organization.ShortName,
                RoleId = x.RoleId,
                RoleName = x.Role != null ? x.Role.FullName : null,
                IsDefault = x.IsDefault,
                IsOwner = x.IsOwner,
                StateId = x.StateId,
                JoinedAt = x.JoinedAt,
                InvitedByUserId = x.InvitedByUserId,
                LastAccessAt = x.LastAccessAt,
                BlockedAt = x.BlockedAt
            }
        }, ct);

    private async Task<List<PlatformUserOrganizationDto>> GetOrganizationMembershipDtosAsync(int organizationId, CancellationToken ct) =>
        await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization, PlatformUserOrganizationDto>
        {
            Criteria = x => x.OrganizationId == organizationId,
            OrderBy = query => query.OrderBy(x => x.UserName),
            Selector = x => new PlatformUserOrganizationDto
            {
                UserId = x.UserId,
                UserName = x.User.UserName,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.Organization.ShortName,
                RoleId = x.RoleId,
                RoleName = x.Role != null ? x.Role.FullName : null,
                IsDefault = x.IsDefault,
                IsOwner = x.IsOwner,
                StateId = x.StateId,
                JoinedAt = x.JoinedAt,
                InvitedByUserId = x.InvitedByUserId,
                LastAccessAt = x.LastAccessAt,
                BlockedAt = x.BlockedAt
            }
        }, ct);

    private async Task ClearUserDefaultOrganizationsAsync(int userId, CancellationToken ct)
    {
        var defaults = await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization>
        {
            Criteria = x => x.UserId == userId && x.IsDefault
        }, ct);

        foreach (var item in defaults)
            item.IsDefault = false;

        if (defaults.Count > 0)
            await _userOrganizationCommand.UpdateAsync(defaults, ct);
    }

    private static void ApplyMembershipCreateDto(UserOrganization membership, PlatformUserOrganizationCreateDto dto, DateTime now)
    {
        membership.RoleId = dto.RoleId;
        membership.IsDefault = dto.IsDefault;
        membership.IsOwner = dto.IsOwner;
        membership.InvitedByUserId = dto.InvitedByUserId;
        membership.StateId = StateIdConst.ACTIVE;
        membership.BlockedAt = null;
        if (membership.JoinedAt == default)
            membership.JoinedAt = now;
    }

    private static PlatformUserOrganizationDto MapMembership(UserOrganization membership) =>
        new()
        {
            UserId = membership.UserId,
            OrganizationId = membership.OrganizationId,
            RoleId = membership.RoleId,
            IsDefault = membership.IsDefault,
            IsOwner = membership.IsOwner,
            StateId = membership.StateId,
            JoinedAt = membership.JoinedAt,
            InvitedByUserId = membership.InvitedByUserId,
            LastAccessAt = membership.LastAccessAt,
            BlockedAt = membership.BlockedAt
        };

    private static PlatformUserDetailDto CreatePlatformUserDetailDto(
        PlatformUserDto dto,
        List<PlatformUserOrganizationDto> organizations) =>
        new()
        {
            Id = dto.Id,
            UserName = dto.UserName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
            EmailVerified = dto.EmailVerified,
            EmailVerifiedAt = dto.EmailVerifiedAt,
            LastLoginIp = dto.LastLoginIp,
            IsPlatformAdmin = dto.IsPlatformAdmin,
            Timezone = dto.Timezone,
            LastAccessTime = dto.LastAccessTime,
            StateId = dto.StateId,
            CreatedDate = dto.CreatedDate,
            RoleName = dto.RoleName,
            HasGlobalAccess = dto.HasGlobalAccess,
            StateName = dto.StateName,
            OrganizationsCount = dto.OrganizationsCount,
            Organizations = organizations
        };

    private static PlatformOrganizationDetailDto CreatePlatformOrganizationDetailDto(
        PlatformOrganizationDto dto,
        List<PlatformUserOrganizationDto> users,
        PlatformWorkspaceSetupDto? setup) =>
        new()
        {
            Id = dto.Id,
            ShortName = dto.ShortName,
            FullName = dto.FullName,
            Inn = dto.Inn,
            PhoneNumber = dto.PhoneNumber,
            RegionId = dto.RegionId,
            RegionName = dto.RegionName,
            DistrictId = dto.DistrictId,
            DistrictName = dto.DistrictName,
            Address = dto.Address,
            Director = dto.Director,
            IsParent = dto.IsParent,
            StateId = dto.StateId,
            StateName = dto.StateName,
            DefaultLanguageId = dto.DefaultLanguageId,
            DefaultLanguageName = dto.DefaultLanguageName,
            TenantId = dto.TenantId,
            SetupStatus = dto.SetupStatus,
            SetupCompletedAt = dto.SetupCompletedAt,
            Email = dto.Email,
            Website = dto.Website,
            Oked = dto.Oked,
            CreatedDate = dto.CreatedDate,
            TenantName = dto.TenantName,
            UsersCount = dto.UsersCount,
            Users = users,
            Setup = setup
        };

    private static PlatformUserDto CreateWorkspaceUserDto(User user) =>
        new()
        {
            Id = user.Id,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleId = user.RoleId,
            StateId = user.StateId
        };

    private static PlatformAuditLogDto CreatePlatformAuditLogDto(
        AuditLogQueryItem log) =>
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

    private static bool HasAccountingSettings(AccountantWorkspaceCreateDto dto) =>
        dto.AccountingPolicyId.HasValue
        || dto.BaseCurrencyId.HasValue
        || dto.AccountingStartDate.HasValue
        || !string.IsNullOrWhiteSpace(dto.InventoryValuationMethod);

    private static bool HasDefaults(Application.Features.OrganizationSetup.OrganizationSetupDefaultsDto? dto) =>
        dto is not null
        && (dto.BranchId.HasValue
            || dto.WarehouseId.HasValue
            || dto.CashBoxId.HasValue
            || dto.BankAccountId.HasValue
            || dto.ReceivableAccountId.HasValue
            || dto.PayableAccountId.HasValue
            || dto.InventoryAccountId.HasValue
            || dto.CashAccountId.HasValue
            || dto.BankAccountingAccountId.HasValue
            || dto.RevenueAccountId.HasValue
            || dto.ExpenseAccountId.HasValue
            || dto.CogsAccountId.HasValue);

    private static string NormalizeSlug(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..12] : slug;
    }

    private static string NormalizeInventoryValuationMethod(string value) => value.Trim().ToLowerInvariant();

    private async Task<int> CountTenantsAsync(System.Linq.Expressions.Expression<Func<PlatformTenant, bool>> criteria, CancellationToken ct)
    {
        var page = await _tenantQuery.GetPagedAsync(new PagedQuerySpecification<PlatformTenant> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountOrganizationsAsync(System.Linq.Expressions.Expression<Func<Organization, bool>> criteria, CancellationToken ct)
    {
        var page = await _organizationQuery.GetPagedAsync(new PagedQuerySpecification<Organization> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountUsersAsync(System.Linq.Expressions.Expression<Func<User, bool>> criteria, CancellationToken ct)
    {
        var page = await _userQuery.GetPagedAsync(new PagedQuerySpecification<User> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }

    private async Task<int> CountUserOrganizationsAsync(System.Linq.Expressions.Expression<Func<UserOrganization, bool>> criteria, CancellationToken ct)
    {
        var page = await _userOrganizationQuery.GetPagedAsync(new PagedQuerySpecification<UserOrganization> { Criteria = criteria, Take = 1 }, ct);
        return page.TotalCount;
    }
}

using System.Text.RegularExpressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
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
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly IQueryRepository<TaxType> _taxTypeQuery;
    private readonly IQueryRepository<AccountingPolicy> _accountingPolicyQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<OrganizationSetupState> _setupStateQuery;
    private readonly IQueryRepository<AuditLog> _auditLogQuery;
    private readonly ICommandRepository<OrganizationSetupState> _setupStateCommand;
    private readonly ICommandRepository<OrganizationTaxSetting> _taxSettingCommand;
    private readonly ICommandRepository<OrganizationConfig> _configCommand;
    private readonly ICommandRepository<OrganizationDefault> _defaultCommand;

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
        IQueryRepository<Role> roleQuery,
        IQueryRepository<TaxType> taxTypeQuery,
        IQueryRepository<AccountingPolicy> accountingPolicyQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<OrganizationSetupState> setupStateQuery,
        IQueryRepository<AuditLog> auditLogQuery,
        ICommandRepository<OrganizationSetupState> setupStateCommand,
        ICommandRepository<OrganizationTaxSetting> taxSettingCommand,
        ICommandRepository<OrganizationConfig> configCommand,
        ICommandRepository<OrganizationDefault> defaultCommand,
        ILogger<PlatformService> logger,
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
        _roleQuery = roleQuery;
        _taxTypeQuery = taxTypeQuery;
        _accountingPolicyQuery = accountingPolicyQuery;
        _currencyQuery = currencyQuery;
        _setupStateQuery = setupStateQuery;
        _auditLogQuery = auditLogQuery;
        _setupStateCommand = setupStateCommand;
        _taxSettingCommand = taxSettingCommand;
        _configCommand = configCommand;
        _defaultCommand = defaultCommand;
    }

    public Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetDashboardAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformDashboardDto>(PlatformErrors.GlobalAccessRequired());

            var tenantsCount = await CountTenantsAsync(_ => true, ct);
            var activeTenantsCount = await CountTenantsAsync(x => x.StateId == StateIdConst.ACTIVE, ct);
            var organizationsCount = await CountOrganizationsAsync(_ => true, ct);
            var activeOrganizationsCount = await CountOrganizationsAsync(x => x.StateId == StateIdConst.ACTIVE, ct);
            var usersCount = await CountUsersAsync(_ => true, ct);
            var activeUsersCount = await CountUsersAsync(x => x.StateId == StateIdConst.ACTIVE, ct);

            return new PlatformDashboardDto
            {
                TenantsCount = tenantsCount,
                ActiveTenantsCount = activeTenantsCount,
                InactiveTenantsCount = tenantsCount - activeTenantsCount,
                OrganizationsCount = organizationsCount,
                ActiveOrganizationsCount = activeOrganizationsCount,
                UsersCount = usersCount,
                ActiveUsersCount = activeUsersCount
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
                Selector = user => new PlatformUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleId = user.RoleId,
                    RoleName = user.Role.FullName,
                    HasGlobalAccess = user.Role.HasGlobalAccess,
                    EmailVerified = user.EmailVerified,
                    EmailVerifiedAt = user.EmailVerifiedAt,
                    IsPlatformAdmin = user.IsPlatformAdmin,
                    Timezone = user.Timezone,
                    LastAccessTime = user.LastAccessTime,
                    StateId = user.StateId,
                    StateName = user.State.FullName,
                    CreatedDate = user.CreatedDate,
                    OrganizationsCount = user.UserOrganizations.Count(uo => uo.StateId == StateIdConst.ACTIVE)
                }
            };

            var paged = await _userQuery.GetPagedAsync(spec, ct);
            return PagedResponseFactory.Create(paged, page, pageSize);
        });

    public Task<Result<PlatformUserDetailDto>> GetUserByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetUserByIdAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.GlobalAccessRequired());

            var user = await _userQuery.GetAsync(new QuerySpecification<User, PlatformUserDetailDto>
            {
                Criteria = x => x.Id == id,
                Selector = x => new PlatformUserDetailDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    RoleId = x.RoleId,
                    RoleName = x.Role.FullName,
                    HasGlobalAccess = x.Role.HasGlobalAccess,
                    EmailVerified = x.EmailVerified,
                    EmailVerifiedAt = x.EmailVerifiedAt,
                    IsPlatformAdmin = x.IsPlatformAdmin,
                    Timezone = x.Timezone,
                    LastAccessTime = x.LastAccessTime,
                    StateId = x.StateId,
                    StateName = x.State.FullName,
                    CreatedDate = x.CreatedDate,
                    OrganizationsCount = x.UserOrganizations.Count(uo => uo.StateId == StateIdConst.ACTIVE)
                }
            }, ct);

            if (user is null)
                return Result.Failure<PlatformUserDetailDto>(PlatformErrors.UserNotFound(id));

            user.Organizations = await GetUserMembershipDtosAsync(id, ct);
            return user;
        });

    public Task<Result<int>> CreateUserAsync(PlatformUserCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateUserAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<int>(PlatformErrors.GlobalAccessRequired());

            var userName = dto.UserName.Trim();
            var exists = await _userQuery.AnyAsync(x => x.UserName == userName, ct);
            if (exists)
                return Result.Failure<int>(PlatformErrors.UserNameConflict(userName));

            var roleValidation = await ValidateRoleAsync(dto.RoleId, ct);
            if (roleValidation is not null)
                return Result.Failure<int>(roleValidation);

            var memberships = NormalizeMemberships(dto.Organizations);
            var membershipValidation = await ValidateMembershipsAsync(memberships, ct);
            if (membershipValidation is not null)
                return Result.Failure<int>(membershipValidation);

            var now = DateTime.Now;
            var salt = _passwordHasher.GenerateSalt();
            var user = new User
            {
                UserName = userName,
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = dto.Email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                RoleId = dto.RoleId,
                LanguageId = dto.LanguageId,
                OrganizationId = GetDefaultOrganizationId(memberships),
                EmailVerified = dto.EmailVerified,
                EmailVerifiedAt = dto.EmailVerified ? now : null,
                IsPlatformAdmin = dto.IsPlatformAdmin,
                Timezone = dto.Timezone,
                PasswordSalt = salt,
                PasswordHash = _passwordHasher.Hash(dto.Password, salt),
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now
            };

            await _userCommand.CreateAsync(user, ct);
            await SyncUserMembershipsAsync(user.Id, memberships, replaceExisting: false, ct);

            return user.Id;
        }, ct);

    public Task<Result> UpdateUserAsync(int id, PlatformUserUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateUserAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var user = await _userQuery.GetAsync(new QuerySpecification<User> { Criteria = x => x.Id == id }, ct);
            if (user is null)
                return Result.Failure(PlatformErrors.UserNotFound(id));

            var userName = dto.UserName.Trim();
            if (!string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _userQuery.AnyAsync(x => x.Id != id && x.UserName == userName, ct);
                if (exists)
                    return Result.Failure(PlatformErrors.UserNameConflict(userName));
            }

            var roleValidation = await ValidateRoleAsync(dto.RoleId, ct);
            if (roleValidation is not null)
                return Result.Failure(roleValidation);

            List<PlatformUserOrganizationCreateDto>? memberships = null;
            if (dto.Organizations is not null)
            {
                memberships = NormalizeMemberships(dto.Organizations);
                var membershipValidation = await ValidateMembershipsAsync(memberships, ct);
                if (membershipValidation is not null)
                    return Result.Failure(membershipValidation);
            }

            var now = DateTime.Now;
            user.UserName = userName;
            user.PhoneNumber = dto.PhoneNumber.Trim();
            user.Email = dto.Email;
            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.RoleId = dto.RoleId;
            user.LanguageId = dto.LanguageId;
            user.EmailVerified = dto.EmailVerified;
            user.EmailVerifiedAt = dto.EmailVerified ? user.EmailVerifiedAt ?? now : null;
            user.IsPlatformAdmin = dto.IsPlatformAdmin;
            user.Timezone = dto.Timezone;
            user.StateId = dto.StateId;

            if (memberships is not null)
                user.OrganizationId = GetDefaultOrganizationId(memberships);

            await _userCommand.UpdateAsync(user, ct);

            if (memberships is not null)
                await SyncUserMembershipsAsync(user.Id, memberships, replaceExisting: true, ct);

            return Result.Success();
        }, ct);

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
            if (!_userContext.HasGlobalAccess)
                return Result.Failure<PlatformOrganizationDetailDto>(PlatformErrors.GlobalAccessRequired());

            var organization = await GetOrganizationEntityAsync(id, ct);
            if (organization is null)
                return Result.Failure<PlatformOrganizationDetailDto>(PlatformErrors.OrganizationNotFound(id));

            var baseDto = await MapOrganizationAsync(organization, ct);
            var setup = await _setupStateQuery.GetAsync(new QuerySpecification<OrganizationSetupState>
            {
                Criteria = x => x.OrganizationId == id
            }, ct);

            return new PlatformOrganizationDetailDto
            {
                Id = baseDto.Id,
                ShortName = baseDto.ShortName,
                FullName = baseDto.FullName,
                Inn = baseDto.Inn,
                PhoneNumber = baseDto.PhoneNumber,
                RegionId = baseDto.RegionId,
                RegionName = baseDto.RegionName,
                DistrictId = baseDto.DistrictId,
                DistrictName = baseDto.DistrictName,
                Address = baseDto.Address,
                Director = baseDto.Director,
                IsParent = baseDto.IsParent,
                StateId = baseDto.StateId,
                StateName = baseDto.StateName,
                DefaultLanguageId = baseDto.DefaultLanguageId,
                DefaultLanguageName = baseDto.DefaultLanguageName,
                TenantId = baseDto.TenantId,
                TenantName = baseDto.TenantName,
                SetupStatus = baseDto.SetupStatus,
                SetupCompletedAt = baseDto.SetupCompletedAt,
                Email = baseDto.Email,
                Website = baseDto.Website,
                Oked = baseDto.Oked,
                CreatedDate = baseDto.CreatedDate,
                UsersCount = baseDto.UsersCount,
                Users = await GetOrganizationMembershipDtosAsync(id, ct),
                Setup = setup is null ? null : MapSetup(setup)
            };
        });

    public Task<Result> UpdateOrganizationAsync(int id, PlatformOrganizationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateOrganizationAsync), async () =>
        {
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var organization = await GetOrganizationEntityAsync(id, ct);
            if (organization is null)
                return Result.Failure(PlatformErrors.OrganizationNotFound(id));

            var inn = dto.Inn.Trim();
            if (!string.Equals(organization.Inn, inn, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _organizationQuery.AnyAsync(x => x.Id != id && x.Inn == inn, ct);
                if (exists)
                    return Result.Failure(PlatformErrors.OrganizationInnConflict(inn));
            }

            if (dto.TenantId.HasValue)
            {
                var tenantExists = await _tenantQuery.AnyAsync(x => x.Id == dto.TenantId.Value, ct);
                if (!tenantExists)
                    return Result.Failure(PlatformErrors.TenantNotFound(dto.TenantId.Value));
            }

            organization.ShortName = dto.ShortName.Trim();
            organization.FullName = dto.FullName.Trim();
            organization.Inn = inn;
            organization.PhoneNumber = dto.PhoneNumber;
            organization.RegionId = dto.RegionId;
            organization.DistrictId = dto.DistrictId;
            organization.Address = dto.Address;
            organization.Director = dto.Director;
            organization.IsParent = dto.IsParent;
            organization.DefaultLanguageId = dto.DefaultLanguageId;
            organization.TenantId = dto.TenantId;
            organization.SetupStatus = string.IsNullOrWhiteSpace(dto.SetupStatus) ? organization.SetupStatus : dto.SetupStatus.Trim();
            organization.SetupCompletedAt = dto.SetupCompletedAt;
            organization.Email = dto.Email;
            organization.Website = dto.Website;
            organization.Oked = dto.Oked;
            organization.StateId = dto.StateId;

            await _organizationCommand.UpdateAsync(organization, ct);
            return Result.Success();
        });

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

            var userNameExists = await _userQuery.AnyAsync(x => x.UserName == dto.UserName, ct);
            if (userNameExists)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.UserNameConflict(dto.UserName));

            var innExists = await _organizationQuery.AnyAsync(x => x.Inn == dto.Inn, ct);
            if (innExists)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.OrganizationInnConflict(dto.Inn));

            var roleExists = await _roleQuery.AnyAsync(x => x.Id == dto.RoleId && x.StateId == StateIdConst.ACTIVE, ct);
            if (!roleExists)
                return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.RoleNotFound(dto.RoleId));

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

            var salt = _passwordHasher.GenerateSalt();
            var user = new User
            {
                UserName = dto.UserName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = dto.Email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                RoleId = dto.RoleId,
                LanguageId = dto.LanguageId,
                EmailVerified = !string.IsNullOrWhiteSpace(dto.Email),
                EmailVerifiedAt = !string.IsNullOrWhiteSpace(dto.Email) ? now : null,
                IsPlatformAdmin = false,
                Timezone = dto.Timezone,
                PasswordSalt = salt,
                PasswordHash = _passwordHasher.Hash(dto.Password, salt),
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now
            };
            await _userCommand.CreateAsync(user, ct);

            tenant.OwnerUserId = user.Id;
            tenant.UpdatedDate = now;
            await _tenantCommand.UpdateAsync(tenant, ct);

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

            user.OrganizationId = organization.Id;
            await _userCommand.UpdateAsync(user, ct);

            var membership = new UserOrganization
            {
                UserId = user.Id,
                OrganizationId = organization.Id,
                RoleId = dto.RoleId,
                IsDefault = true,
                IsOwner = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                JoinedAt = now
            };
            await _userOrganizationCommand.CreateAsync(membership, ct);

            if (hasTax && dto.TaxTypeId.HasValue)
            {
                await _taxSettingCommand.CreateAsync(new OrganizationTaxSetting
                {
                    OrganizationId = organization.Id,
                    TaxTypeId = dto.TaxTypeId.Value,
                    IsVatPayer = dto.IsVatPayer,
                    VatRegistrationNumber = dto.VatRegistrationNumber,
                    EffectiveFrom = dto.TaxEffectiveFrom ?? DateOnly.FromDateTime(now),
                    EffectiveTo = dto.TaxEffectiveTo,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = now
                }, ct);
            }

            if (hasAccounting)
            {
                var method = NormalizeInventoryValuationMethod(dto.InventoryValuationMethod ?? "fifo");
                if (!InventoryValuationMethods.Contains(method))
                    return Result.Failure<AccountantWorkspaceDto>(PlatformErrors.InvalidInventoryValuationMethod(method));

                await _configCommand.CreateAsync(new OrganizationConfig
                {
                    OrganizationId = organization.Id,
                    InventoryValuationMethod = method,
                    AccountingPolicyId = dto.AccountingPolicyId,
                    BaseCurrencyId = dto.BaseCurrencyId,
                    AccountingStartDate = dto.AccountingStartDate,
                    FiscalYearStartMonth = dto.FiscalYearStartMonth <= 0 ? (short)1 : dto.FiscalYearStartMonth
                }, ct);
            }

            if (hasDefaults && dto.Defaults is not null)
                await _defaultCommand.CreateAsync(CreateDefaultsEntity(organization.Id, dto.Defaults, now), ct);

            await _setupStateCommand.CreateAsync(new OrganizationSetupState
            {
                OrganizationId = organization.Id,
                CurrentStep = ResolveCurrentStep(true, hasTax, hasAccounting, hasDefaults, true, isCompleted),
                OrganizationCompleted = true,
                TaxCompleted = hasTax,
                AccountingCompleted = hasAccounting,
                DefaultsCompleted = hasDefaults,
                UsersCompleted = true,
                IsCompleted = isCompleted,
                CompletedAt = isCompleted ? now : null,
                CreatedDate = now,
                UpdatedDate = now
            }, ct);

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
            var tableName = filter.TableName?.Trim().ToLowerInvariant();
            var recordId = filter.RecordId?.Trim().ToLowerInvariant();
            var action = filter.Action?.Trim().ToLowerInvariant();

            var spec = new PagedQuerySpecification<AuditLog>
            {
                Criteria = log =>
                    (!filter.OrganizationId.HasValue || log.OrganizationId == filter.OrganizationId.Value)
                    && (!filter.ChangedUserId.HasValue || log.ChangedUserId == filter.ChangedUserId.Value)
                    && (string.IsNullOrWhiteSpace(tableName) || log.TableName.ToLower().Contains(tableName))
                    && (string.IsNullOrWhiteSpace(recordId) || (log.RecordId != null && log.RecordId.ToLower().Contains(recordId)))
                    && (string.IsNullOrWhiteSpace(action) || log.Action.ToLower() == action)
                    && (!filter.FromDate.HasValue || log.ChangedDate >= filter.FromDate.Value)
                    && (!filter.ToDate.HasValue || log.ChangedDate <= filter.ToDate.Value),
                OrderBy = query => query.OrderByDescending(log => log.ChangedDate),
                Skip = (page - 1) * pageSize,
                Take = pageSize
            };

            var paged = await _auditLogQuery.GetPagedAsync(spec, ct);
            var items = await MapAuditLogsAsync(paged.Items, ct);

            return PagedResponseFactory.Create(new PagedList<PlatformAuditLogDto>(items, paged.TotalCount), page, pageSize);
        });

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
            if (!_userContext.HasGlobalAccess)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            var organization = await GetOrganizationEntityAsync(id, ct);
            if (organization is null)
                return Result.Failure(PlatformErrors.OrganizationNotFound(id));

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

    private async Task<Organization?> GetOrganizationEntityAsync(int id, CancellationToken ct)
    {
        var spec = new QuerySpecification<Organization> { Criteria = x => x.Id == id };
        spec.AddIncludes(b =>
        {
            b.Include(x => x.Region);
            b.Include(x => x.District);
            b.Include(x => x.State);
            b.Include(x => x.DefaultLanguage);
        });

        return await _organizationQuery.GetAsync(spec, ct);
    }

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
        var tenantName = organization.TenantId.HasValue
            ? await _tenantQuery.GetAsync(new QuerySpecification<PlatformTenant, string>
            {
                Criteria = x => x.Id == organization.TenantId.Value,
                Selector = x => x.Name
            }, ct)
            : null;

        var usersCount = await CountUserOrganizationsAsync(x => x.OrganizationId == organization.Id && x.StateId == StateIdConst.ACTIVE, ct);

        return new PlatformOrganizationDto
        {
            Id = organization.Id,
            ShortName = organization.ShortName,
            FullName = organization.FullName,
            Inn = organization.Inn,
            PhoneNumber = organization.PhoneNumber,
            RegionId = organization.RegionId,
            RegionName = organization.Region?.FullName,
            DistrictId = organization.DistrictId,
            DistrictName = organization.District?.FullName,
            Address = organization.Address,
            Director = organization.Director,
            IsParent = organization.IsParent,
            StateId = organization.StateId,
            StateName = organization.State?.FullName,
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
        var organization = await _organizationQuery.GetAsync(new QuerySpecification<Organization>
        {
            Criteria = x => x.Id == organizationId
        }, ct);

        if (organization is null || organization.TenantId is null)
            return null;

        var tenant = await GetTenantEntityAsync(organization.TenantId.Value, ct);
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
            User = new PlatformUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId,
                RoleName = null,
                StateId = user.StateId
            },
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

    private async Task<Error?> ValidateRoleAsync(int roleId, CancellationToken ct)
    {
        var roleExists = await _roleQuery.AnyAsync(x => x.Id == roleId && x.StateId == StateIdConst.ACTIVE, ct);
        return roleExists ? null : PlatformErrors.RoleNotFound(roleId);
    }

    private async Task<Error?> ValidateMembershipsAsync(List<PlatformUserOrganizationCreateDto> memberships, CancellationToken ct)
    {
        foreach (var membership in memberships)
        {
            var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == membership.OrganizationId, ct);
            if (!organizationExists)
                return PlatformErrors.OrganizationNotFound(membership.OrganizationId);

            if (membership.RoleId.HasValue)
            {
                var roleValidation = await ValidateRoleAsync(membership.RoleId.Value, ct);
                if (roleValidation is not null)
                    return roleValidation;
            }

            if (membership.InvitedByUserId.HasValue)
            {
                var userExists = await _userQuery.AnyAsync(x => x.Id == membership.InvitedByUserId.Value, ct);
                if (!userExists)
                    return PlatformErrors.UserNotFound(membership.InvitedByUserId.Value);
            }
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

    private async Task SyncUserMembershipsAsync(int userId, List<PlatformUserOrganizationCreateDto> memberships, bool replaceExisting, CancellationToken ct)
    {
        var now = DateTime.Now;
        var existing = await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization>
        {
            Criteria = x => x.UserId == userId
        }, ct);

        var requestedOrgIds = memberships.Select(x => x.OrganizationId).ToHashSet();
        var toUpdate = new List<UserOrganization>();
        var toCreate = new List<UserOrganization>();

        if (replaceExisting)
        {
            foreach (var item in existing.Where(x => !requestedOrgIds.Contains(x.OrganizationId)))
            {
                item.IsDefault = false;
                item.StateId = StateIdConst.PASSIVE;
                item.BlockedAt ??= now;
                toUpdate.Add(item);
            }
        }

        foreach (var dto in memberships)
        {
            var item = existing.FirstOrDefault(x => x.OrganizationId == dto.OrganizationId);
            if (item is null)
            {
                item = new UserOrganization
                {
                    UserId = userId,
                    OrganizationId = dto.OrganizationId,
                    CreatedDate = now,
                    JoinedAt = now
                };
                ApplyMembershipCreateDto(item, dto, now);
                toCreate.Add(item);
            }
            else
            {
                ApplyMembershipCreateDto(item, dto, now);
                toUpdate.Add(item);
            }
        }

        if (toCreate.Count > 0)
            await _userOrganizationCommand.CreateAsync(toCreate, ct);

        if (toUpdate.Count > 0)
            await _userOrganizationCommand.UpdateAsync(toUpdate, ct);
    }

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

    private static List<PlatformUserOrganizationCreateDto> NormalizeMemberships(IEnumerable<PlatformUserOrganizationCreateDto> memberships)
    {
        var normalized = memberships
            .Where(x => x.OrganizationId > 0)
            .GroupBy(x => x.OrganizationId)
            .Select(x => x.First())
            .Select(x => new PlatformUserOrganizationCreateDto
            {
                OrganizationId = x.OrganizationId,
                RoleId = x.RoleId,
                IsDefault = x.IsDefault,
                IsOwner = x.IsOwner,
                InvitedByUserId = x.InvitedByUserId
            })
            .ToList();

        if (normalized.Count == 0)
            return normalized;

        var defaultAssigned = false;
        foreach (var item in normalized)
        {
            if (!defaultAssigned && item.IsDefault)
            {
                defaultAssigned = true;
                continue;
            }

            item.IsDefault = false;
        }

        if (!defaultAssigned)
            normalized[0].IsDefault = true;

        return normalized;
    }

    private static int? GetDefaultOrganizationId(List<PlatformUserOrganizationCreateDto> memberships) =>
        memberships.FirstOrDefault(x => x.IsDefault)?.OrganizationId
        ?? memberships.FirstOrDefault()?.OrganizationId;

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

    private async Task<List<PlatformAuditLogDto>> MapAuditLogsAsync(List<AuditLog> logs, CancellationToken ct)
    {
        var userIds = logs
            .Where(x => x.ChangedUserId.HasValue)
            .Select(x => x.ChangedUserId!.Value)
            .Distinct()
            .ToList();

        var organizationIds = logs
            .Where(x => x.OrganizationId.HasValue)
            .Select(x => x.OrganizationId!.Value)
            .Distinct()
            .ToList();

        var users = userIds.Count == 0
            ? new List<User>()
            : await _userQuery.GetAllAsync(new QuerySpecification<User>
            {
                Criteria = x => userIds.Contains(x.Id)
            }, ct);

        var organizations = organizationIds.Count == 0
            ? new List<Organization>()
            : await _organizationQuery.GetAllAsync(new QuerySpecification<Organization>
            {
                Criteria = x => organizationIds.Contains(x.Id)
            }, ct);

        var userNames = users.ToDictionary(x => x.Id, x => x.UserName);
        var organizationNames = organizations.ToDictionary(x => x.Id, x => x.ShortName);

        return logs.Select(log => new PlatformAuditLogDto
        {
            Id = log.Id,
            OrganizationId = log.OrganizationId,
            OrganizationName = log.OrganizationId.HasValue && organizationNames.TryGetValue(log.OrganizationId.Value, out var organizationName)
                ? organizationName
                : null,
            SchemaName = log.SchemaName,
            TableName = log.TableName,
            RecordId = log.RecordId,
            Action = log.Action,
            OldData = log.OldData,
            NewData = log.NewData,
            ChangedUserId = log.ChangedUserId,
            ChangedUserName = log.ChangedUserId.HasValue && userNames.TryGetValue(log.ChangedUserId.Value, out var userName)
                ? userName
                : null,
            RequestId = log.RequestId,
            ClientAddr = log.ClientAddr?.ToString(),
            ApplicationName = log.ApplicationName,
            ChangedDate = log.ChangedDate
        }).ToList();
    }

    private static OrganizationDefault CreateDefaultsEntity(int organizationId, Application.Features.OrganizationSetup.OrganizationSetupDefaultsDto dto, DateTime now) =>
        new()
        {
            OrganizationId = organizationId,
            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,
            CashBoxId = dto.CashBoxId,
            BankAccountId = dto.BankAccountId,
            ReceivableAccountId = dto.ReceivableAccountId,
            PayableAccountId = dto.PayableAccountId,
            InventoryAccountId = dto.InventoryAccountId,
            CashAccountId = dto.CashAccountId,
            BankAccountingAccountId = dto.BankAccountingAccountId,
            RevenueAccountId = dto.RevenueAccountId,
            ExpenseAccountId = dto.ExpenseAccountId,
            CogsAccountId = dto.CogsAccountId,
            CreatedDate = now
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

    private static string ResolveCurrentStep(
        bool organizationCompleted,
        bool taxCompleted,
        bool accountingCompleted,
        bool defaultsCompleted,
        bool usersCompleted,
        bool isCompleted)
    {
        if (isCompleted)
            return "complete";
        if (!organizationCompleted)
            return "company-profile";
        if (!taxCompleted)
            return "tax-settings";
        if (!accountingCompleted)
            return "accounting-policy";
        if (!defaultsCompleted)
            return "defaults";
        if (!usersCompleted)
            return "users";
        return "complete";
    }

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

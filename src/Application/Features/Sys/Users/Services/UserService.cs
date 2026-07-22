using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Users.Services;

public class UserService : BaseService, IUserService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryRepository<UserOrganization> _userOrgQuery;
    private readonly IUserManagementCore _userManagementCore;

    public UserService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<User> userQuery,
        ICommandRepository<User> userCommand,
        IQueryRepository<UserOrganization> userOrgQuery,
        IUserManagementCore userManagementCore,
        ILogger<UserService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userOrgQuery = userOrgQuery;
        _userManagementCore = userManagementCore;
    }

    public async Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.TenantId is null)
            return Result.Failure<int>(CommonErrors.UserHasNoTenant(_userContext.LanguageId));

        UserWelcomeEmailMessage? welcomeEmail = null;

        var result = await ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            var user = MapCreateRequest(dto, _userContext.TenantId!.Value);

            var coreResult = await _userManagementCore.CreateUserAsync(
                user,
                UserManagementOptions.ForOrganization(sendWelcomeEmail: true),
                ct);

            if (!coreResult.IsSuccess)
                return Result.Failure<int>(coreResult.Error);

            welcomeEmail = coreResult.Value.WelcomeEmail;

            return coreResult.Value.UserId;
        }, ct);

        if (result.IsSuccess && welcomeEmail is not null)
            await _userManagementCore.SendWelcomeEmailSafeAsync(welcomeEmail, ct);

        return result;
    }

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<User>().Where(x => x.Id == id).Build();
            var entity = await _userQuery.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(UserErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;

            await _userCommand.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<UserListDto>>> GetAllAsync(UserListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<User, UserListDto, UserListFilter>(filter);
            var pagedList = await _userQuery.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<User>().Where(x => id == x.Id).As<UserDto>().Build();
            var entity = await _userQuery.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<UserDto>(UserErrors.NotFound(id, _userContext.LanguageId));

            var orgSpec = new QuerySpecification<UserOrganization, UserOrganizationItemDto>
            {
                Criteria = uo => uo.UserId == id && uo.StateId == StateIdConst.ACTIVE,
                Selector = uo => new UserOrganizationItemDto
                {
                    OrganizationId = uo.OrganizationId,
                    OrganizationName = uo.Organization.ShortName,
                    RoleId = uo.RoleId,
                    RoleName = uo.Role != null ? uo.Role.FullName : null,
                    IsDefault = uo.IsDefault,
                    IsOwner = uo.IsOwner,
                    JoinedAt = uo.JoinedAt,
                    InvitedByUserId = uo.InvitedByUserId,
                    LastAccessAt = uo.LastAccessAt,
                    BlockedAt = uo.BlockedAt
                }
            };
            entity.Organizations = await _userOrgQuery.GetAllAsync(orgSpec, ct);

            return entity;
        });

    public Task<Result> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), () =>
            _userManagementCore.UpdateUserAsync(
                MapUpdateRequest(id, dto),
                UserManagementOptions.ForOrganization(sendWelcomeEmail: false),
                ct), ct);

    private static UserManagementCreateRequest MapCreateRequest(UserCreateDto dto, int tenantId) =>
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
            EmailVerified = dto.EmailVerified,
            IsPlatformAdmin = dto.IsPlatformAdmin,
            Timezone = dto.Timezone,
            Organizations = dto.Organizations
                .Select(orgId => new UserManagementMembershipRequest
                {
                    OrganizationId = orgId
                })
                .ToList()
        };

    private static UserManagementUpdateRequest MapUpdateRequest(int id, UserUpdateDto dto) =>
        new()
        {
            UserId = id,
            UserName = dto.UserName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
            EmailVerified = dto.EmailVerified,
            IsPlatformAdmin = dto.IsPlatformAdmin,
            Timezone = dto.Timezone,
            StateId = dto.StateId,
            Organizations = dto.Organizations
                .Select(orgId => new UserManagementMembershipRequest
                {
                    OrganizationId = orgId
                })
                .ToList()
        };
}

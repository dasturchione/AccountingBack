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
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly IUserManagementCore _userManagementCore;

    public UserService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<User> userQuery,
        ICommandRepository<User> userCommand,
        IQueryRepository<UserOrganization> userOrganizationQuery,
        IUserManagementCore userManagementCore,
        ILogger<UserService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userOrganizationQuery = userOrganizationQuery;
        _userManagementCore = userManagementCore;
    }

    public async Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.TenantId is null)
            return Result.Failure<int>(CommonErrors.UserHasNoTenant(_userContext.LanguageId));

        UserWelcomeEmailMessage? welcomeEmail = null;
        var result = await ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            var coreResult = await _userManagementCore.CreateUserAsync(
                MapCreateRequest(dto, _userContext.TenantId.Value),
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
            var query = _queryBuilder.For<User>().Where(user => user.Id == id).Build();
            var entity = await _userQuery.GetAsync(query, ct);
            if (entity is null)
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
            var query = _queryBuilder.For<User>().Where(user => user.Id == id).As<UserDto>().Build();
            var entity = await _userQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure<UserDto>(UserErrors.NotFound(id, _userContext.LanguageId));

            var organizationSpec = new QuerySpecification<UserOrganization, UserOrganizationItemDto>
            {
                Criteria = membership => membership.UserId == id && membership.StateId == StateIdConst.ACTIVE,
                Selector = membership => new UserOrganizationItemDto
                {
                    OrganizationId = membership.OrganizationId,
                    OrganizationName = membership.Organization.ShortName,
                    RoleId = membership.RoleId,
                    RoleName = membership.Role != null ? membership.Role.FullName : null,
                    IsDefault = membership.IsDefault,
                    IsOwner = membership.IsOwner,
                    JoinedAt = membership.JoinedAt,
                    InvitedByUserId = membership.InvitedByUserId,
                    LastAccessAt = membership.LastAccessAt,
                    BlockedAt = membership.BlockedAt
                }
            };
            entity.Organizations = await _userOrganizationQuery.GetAllAsync(organizationSpec, ct);
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
            UserKindId = UserKindIdConst.TenantUser,
            UserName = dto.UserName,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailVerified = dto.EmailVerified,
            Timezone = dto.Timezone,
            Organizations = MapMemberships(dto.Organizations)
        };

    private static UserManagementUpdateRequest MapUpdateRequest(int id, UserUpdateDto dto) =>
        new()
        {
            UserId = id,
            UserKindId = UserKindIdConst.TenantUser,
            UserName = dto.UserName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailVerified = dto.EmailVerified,
            Timezone = dto.Timezone,
            StateId = dto.StateId,
            Organizations = MapMemberships(dto.Organizations)
        };

    private static List<UserManagementMembershipRequest> MapMemberships(
        IEnumerable<UserOrganizationRequestDto> organizations) =>
        organizations.Select(organization => new UserManagementMembershipRequest
        {
            OrganizationId = organization.OrganizationId,
            RoleId = organization.RoleId,
            IsDefault = organization.IsDefault,
            IsOwner = organization.IsOwner,
            InvitedByUserId = organization.InvitedByUserId
        }).ToList();
}
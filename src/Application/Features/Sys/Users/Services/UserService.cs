using Domain.Entities;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Services;

public class UserService : BaseService, IUserService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryRepository<UserOrganization> _userOrgQuery;
    private readonly ICommandRepository<UserOrganization> _userOrgCommand;
    public UserService(IUserContext userContext,
                       IQueryBuilder queryBuilder,
                       IPasswordHasher passwordHasher,
                       IQueryRepository<User> userQuery,
                       ICommandRepository<User> userCommand,
                       IQueryRepository<UserOrganization> userOrgQuery,
                       ICommandRepository<UserOrganization> userOrgCommand,
                       ILogger<UserService> logger, 
                       IUnitOfWork unitOfWork) 
            : base(logger, unitOfWork)
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _passwordHasher = passwordHasher;
        _userOrgQuery = userOrgQuery;
        _userOrgCommand = userOrgCommand;
    }

    public Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            var exists = await _userQuery.AnyAsync(x => x.UserName == dto.UserName, ct);
            if (exists)
                return Result.Failure<int>(UserErrors.Conflict(dto.UserName, _userContext.LanguageId));

            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(dto.Password, salt);

            var user = new User
            {
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                RoleId = dto.RoleId,
                PasswordSalt = salt,
                PasswordHash = hash,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _userCommand.CreateAsync(user, ct);

            if (dto.Organizations.Count > 0)
            {
                var userOrgs = dto.Organizations.Select(o => new UserOrganization
                {
                    UserId = user.Id,
                    OrganizationId = o.OrganizationId,
                    RoleId = o.RoleId,
                    IsDefault = o.IsDefault,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                });

                await _userOrgCommand.CreateAsync(userOrgs, ct);
            }

            return user.Id;
        });

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
                    IsDefault = uo.IsDefault
                }
            };
            entity.Organizations = await _userOrgQuery.GetAllAsync(orgSpec, ct);

            return entity;
        });

    public Task<Result> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            var query = _queryBuilder.For<User>().Where(x => id == x.Id).Build();
            var user = await _userQuery.GetAsync(query, ct);

            if (user is null)
                return Result.Failure(UserErrors.NotFound(id, _userContext.LanguageId));

            if (user.UserName != dto.UserName)
            {
                var exists = await _userQuery.AnyAsync(u => u.UserName == dto.UserName, ct);
                if (exists)
                    return Result.Failure(UserErrors.Conflict(dto.UserName, _userContext.LanguageId));
            }

            user.UserName = dto.UserName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.RoleId = dto.RoleId;
            user.StateId = dto.StateId;

            await _userCommand.UpdateAsync(user, ct);

            await _userOrgCommand.DeleteAsync(uo => uo.UserId == id, ct);

            if (dto.Organizations.Count > 0)
            {
                var userOrgs = dto.Organizations.Select(o => new UserOrganization
                {
                    UserId = id,
                    OrganizationId = o.OrganizationId,
                    RoleId = o.RoleId,
                    IsDefault = o.IsDefault,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                });

                await _userOrgCommand.CreateAsync(userOrgs, ct);
            }

            return Result.Success();
        });
}

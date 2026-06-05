using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Options;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IUserContext _userContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryBuilder<User> _queryBuilder;
    public UserService(IUserContext userContext,
                       IPasswordHasher passwordHasher,
                       IQueryRepository<User> userQuery,
                       ICommandRepository<User> userCommand,
                       IQueryBuilder<User> queryBuilder)
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userContext = userContext;
        _passwordHasher = passwordHasher;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
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
        return user.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var spec = _queryBuilder.ById(id);
        var entity = await _userQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(UserErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _userCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<UserListDto>>> GetAllAsync(UserListFilter filter, CancellationToken ct = default)
    {
        var spec = _queryBuilder.BuildPaged<UserListDto, UserListFilter>(filter);
        var pagedList = await _userQuery.GetPagedAsync(spec, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var spec = _queryBuilder.Build<UserDto, GetByIdOptions<int>>(new GetByIdOptions<int>(id));
        var entity = await _userQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure<UserDto>(UserErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default)
    {
        var spec = _queryBuilder.ById(id);
        var user = await _userQuery.GetAsync(spec, ct);

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
        return Result.Success();
    }
}

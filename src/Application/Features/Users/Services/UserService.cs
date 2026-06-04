using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Factory;
using Application.Common.Pagination;
using Application.Options;
using Application.Specifications;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISpecificationFactory<User> _userSpecification;
    public UserService(IUserContext userContext,
                       IQueryRepository<User> userQuery,
                       ICommandRepository<User> userCommand,
                       ISpecificationFactory<User> userSpecification,
                       IPasswordHasher passwordHasher   )
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userContext = userContext;
        _userSpecification = userSpecification;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
    {
        var existsSpec = new QuerySpecification<User>
        {
            Criteria = u => u.UserName == dto.UserName
        };

        var exists = await _userQuery.AnyAsync(existsSpec.Criteria, ct);
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
        var spec = _userSpecification.Build(new GetByIdOptions<int>(id));
        var entity = await _userQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(UserErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _userCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<UserListDto>>> GetAllAsync(UserListFilter filter, CancellationToken ct = default)
    {
        var spec = _userSpecification.BuildPaged<UserListDto, UserListFilter>(filter);
        var pagedList = await _userQuery.GetPagedAsync(spec, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var spec = _userSpecification.Build<UserDto, GetByIdOptions<int>>(new GetByIdOptions<int>(id));
        var entity = await _userQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure<UserDto>(UserErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default)
    {
        var spec = _userSpecification.Build(new GetByIdOptions<int>(id));
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

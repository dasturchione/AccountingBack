using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Specifications;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IUserContext _userContext;
    private readonly ICommandRepository<User> _userCommand;

    public AuthService(
        ITokenProvider tokenProvider,
        IPasswordHasher passwordHasher,
        IQueryRepository<User> userQuery,
        ICommandRepository<User> userCommand,
        IUserContext userContext)
    {
        _tokenProvider = tokenProvider;
        _passwordHasher = passwordHasher;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userContext = userContext;
    }

    public async ValueTask<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var spec = new QuerySpecification<User>
        {
            Criteria = u => u.UserName == dto.UserName
        };

        spec.AddIncludes(b =>
        {
            b.Include(u => u.Role);
            b.Include(u => u.State);
        });

        var user = await _userQuery.GetAsync(spec, ct);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordSalt, user.PasswordHash))
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));

        var token = _tokenProvider.GenerateAccessToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            User = new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId,
                RoleName = user.Role.FullName,
                StateName = user.State.ShortName,
                StateId = user.StateId,
                LastAccessTime = user.LastAccessTime,
                CreatedDate = user.CreatedDate
            }
        };

        user.LastAccessTime = DateTime.Now;
        await _userCommand.UpdateAsync(user, ct);

        return response;
    }
}
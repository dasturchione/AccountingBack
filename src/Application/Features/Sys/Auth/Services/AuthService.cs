using Domain.Entities;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly ITokenProvider _tokenProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryRepository<RoleModule> _roleModuleQuery;
    private readonly IQueryRepository<UserOrganization> _userOrgQuery;

    public AuthService(IUserContext userContext,
                       IQueryBuilder queryBuilder,
                       ITokenProvider tokenProvider,
                       IPasswordHasher passwordHasher,
                       IQueryRepository<User> userQuery,
                       ICommandRepository<User> userCommand,
                       IQueryRepository<RoleModule> roleModuleQuery,
                       IQueryRepository<UserOrganization> userOrgQuery)
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _tokenProvider = tokenProvider;
        _passwordHasher = passwordHasher;
        _roleModuleQuery = roleModuleQuery;
        _userOrgQuery = userOrgQuery;
    }

    public async ValueTask<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<User>().Where(x => x.UserName == dto.UserName).Build();

        query.AddIncludes(b =>
        {
            b.Include(u => u.Role);
            b.Include(u => u.State);
        });

        var user = await _userQuery.GetAsync(query, ct);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordSalt, user.PasswordHash))
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));

        // Load user's organizations
        var orgSpec = new QuerySpecification<UserOrganization, UserOrgDto>
        {
            Criteria = uo => uo.UserId == user.Id && uo.StateId == StateIdConst.ACTIVE,
            Selector = uo => new UserOrgDto
            {
                OrganizationId = uo.OrganizationId,
                OrganizationName = uo.Organization.ShortName,
                RoleId = uo.RoleId,
                RoleName = uo.Role != null ? uo.Role.FullName : null,
                IsDefault = uo.IsDefault
            }
        };
        var organizations = await _userOrgQuery.GetAllAsync(orgSpec, ct);

        // Default organization for token — IsDefault=true bo'lgani, bo'lmasa birinchisi
        var defaultOrg = organizations.FirstOrDefault(o => o.IsDefault)
                      ?? organizations.FirstOrDefault();
        var defaultOrgId = defaultOrg?.OrganizationId ?? 0;

        var token = _tokenProvider.GenerateAccessToken(user, defaultOrgId);

        // Load permissions
        var permSpec = new QuerySpecification<RoleModule, string>
        {
            Criteria = rm => rm.RoleId == user.RoleId,
            Selector = rm => rm.Module.Code
        };
        var permissions = await _roleModuleQuery.GetAllAsync(permSpec, ct);

        var response = new LoginResponseDto
        {
            Token = token,
            User = new UserResponseDto
            {
                Id             = user.Id,
                UserName       = user.UserName,
                PhoneNumber    = user.PhoneNumber,
                Email          = user.Email,
                FirstName      = user.FirstName,
                LastName       = user.LastName,
                RoleId         = user.RoleId,
                RoleName       = user.Role.FullName,
                StateName      = user.State.ShortName,
                StateId        = user.StateId,
                LastAccessTime = user.LastAccessTime,
                CreatedDate    = user.CreatedDate,
                Organizations  = organizations,
                Permissions    = permissions
            }
        };

        user.LastAccessTime = DateTime.Now;
        await _userCommand.UpdateAsync(user, ct);

        return response;
    }
}

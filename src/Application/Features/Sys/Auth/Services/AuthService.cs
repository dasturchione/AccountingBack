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
    private readonly IQueryRepository<Module> _moduleQuery;

    public AuthService(IUserContext userContext,
                       IQueryBuilder queryBuilder,
                       ITokenProvider tokenProvider,
                       IPasswordHasher passwordHasher,
                       IQueryRepository<User> userQuery,
                       ICommandRepository<User> userCommand,
                       IQueryRepository<RoleModule> roleModuleQuery,
                       IQueryRepository<UserOrganization> userOrgQuery,
                       IQueryRepository<Module> moduleQuery)
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _tokenProvider = tokenProvider;
        _passwordHasher = passwordHasher;
        _roleModuleQuery = roleModuleQuery;
        _userOrgQuery = userOrgQuery;
        _moduleQuery = moduleQuery;
    }

    public ValueTask<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
        => AuthenticateAsync(dto, requireGlobalAccess: false, ct);

    public ValueTask<Result<LoginResponseDto>> SuperAdminLoginAsync(LoginDto dto, CancellationToken ct = default)
        => AuthenticateAsync(dto, requireGlobalAccess: true, ct);

    private async ValueTask<Result<LoginResponseDto>> AuthenticateAsync(LoginDto dto, bool requireGlobalAccess, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<User>().Where(x => x.UserName == dto.UserName).Build();

        query.AddIncludes(b =>
        {
            b.Include(u => u.Role);
            b.Include(u => u.State);
        });

        var user = await _userQuery.GetAsync(query, ct);

        if (user is null ||
            user.StateId != StateIdConst.ACTIVE ||
            !_passwordHasher.Verify(dto.Password, user.PasswordSalt, user.PasswordHash))
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));

        var hasGlobalAccess = user.Role.HasGlobalAccess;
        if (hasGlobalAccess != requireGlobalAccess)
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));

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

        if (!hasGlobalAccess && organizations.Count == 0)
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));

        var defaultOrg = organizations.FirstOrDefault(o => o.IsDefault)
                      ?? organizations.FirstOrDefault();
        var defaultOrgId = defaultOrg?.OrganizationId ?? 0;

        var token = _tokenProvider.GenerateAccessToken(user, defaultOrgId);

        var permissions = hasGlobalAccess
            ? await GetAllActivePermissionCodesAsync(ct)
            : await GetRolePermissionCodesAsync(user.RoleId, ct);

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
                HasGlobalAccess = hasGlobalAccess,
                StateName = user.State.ShortName,
                StateId = user.StateId,
                LastAccessTime = user.LastAccessTime,
                CreatedDate = user.CreatedDate,
                Organizations = organizations,
                Permissions = permissions
            }
        };

        user.LastAccessTime = DateTime.Now;
        await _userCommand.UpdateAsync(user, ct);

        return response;
    }

    private async Task<List<string>> GetRolePermissionCodesAsync(int roleId, CancellationToken ct)
    {
        var permSpec = new QuerySpecification<RoleModule, string>
        {
            Criteria = rm => rm.RoleId == roleId,
            Selector = rm => rm.Module.Code
        };

        var permissions = await _roleModuleQuery.GetAllAsync(permSpec, ct);
        return permissions.ToList();
    }

    private async Task<List<string>> GetAllActivePermissionCodesAsync(CancellationToken ct)
    {
        var permSpec = new QuerySpecification<Module, string>
        {
            Criteria = module => module.StateId == StateIdConst.ACTIVE,
            Selector = module => module.Code
        };

        var permissions = await _moduleQuery.GetAllAsync(permSpec, ct);
        return permissions.ToList();
    }
}

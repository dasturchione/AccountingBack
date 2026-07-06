using Domain.Entities;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserContext userContext,
                       IQueryBuilder queryBuilder,
                       ITokenProvider tokenProvider,
                       IPasswordHasher passwordHasher,
                       IQueryRepository<User> userQuery,
                       ICommandRepository<User> userCommand,
                       IQueryRepository<RoleModule> roleModuleQuery,
                       IQueryRepository<UserOrganization> userOrgQuery,
                       IQueryRepository<Module> moduleQuery,
                       ILogger<AuthService> logger)
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
        _logger = logger;
    }

    public ValueTask<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
        => AuthenticateAsync(dto, requireGlobalAccess: false, ct);

    public ValueTask<Result<LoginResponseDto>> SuperAdminLoginAsync(LoginDto dto, CancellationToken ct = default)
        => AuthenticateAsync(dto, requireGlobalAccess: true, ct);

    private async ValueTask<Result<LoginResponseDto>> AuthenticateAsync(LoginDto dto, bool requireGlobalAccess, CancellationToken ct = default)
    {
        var normalizedUserName = dto.UserName.Trim();

        var query = _queryBuilder.For<User>().Where(x => x.UserName == normalizedUserName).Build();
        query = new QuerySpecification<User>
        {
            Criteria = query.Criteria,
            OrderBy = query.OrderBy,
            IgnoreQueryFilters = true
        };

        query.AddIncludes(b =>
        {
            b.Include(u => u.Role);
            b.Include(u => u.State);
            b.Include(u => u.Organization);
        });

        var user = await _userQuery.GetAsync(query, ct);

        if (user is null || user.Role is null || user.State is null || user.StateId != StateIdConst.ACTIVE)
        {
            _logger.LogInformation(
                "Authentication failed for {UserName}: user not found, inactive, or missing role/state.",
                normalizedUserName);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

        bool passwordMatches;
        try
        {
            passwordMatches = _passwordHasher.Verify(dto.Password, user.PasswordSalt, user.PasswordHash);
        }
        catch (FormatException)
        {
            _logger.LogWarning("Authentication failed for {UserName}: password salt/hash format is invalid.", normalizedUserName);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }
        catch (CryptographicException)
        {
            _logger.LogWarning("Authentication failed for {UserName}: password verification threw a cryptographic exception.", normalizedUserName);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

        if (!passwordMatches)
        {
            _logger.LogInformation("Authentication failed for {UserName}: password mismatch.", normalizedUserName);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

        var hasGlobalAccess = user.Role.HasGlobalAccess;
        if (hasGlobalAccess != requireGlobalAccess)
        {
            _logger.LogInformation(
                "Authentication failed for {UserName}: role global-access flag {HasGlobalAccess} does not match endpoint requirement {RequireGlobalAccess}.",
                normalizedUserName,
                hasGlobalAccess,
                requireGlobalAccess);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

        var orgSpec = new QuerySpecification<UserOrganization, UserOrgDto>
        {
            Criteria = uo => uo.UserId == user.Id && uo.StateId == StateIdConst.ACTIVE,
            IgnoreQueryFilters = true,
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

        // Legacy databases may still keep the default organization on sys_user.organization_id
        // without a corresponding sys_user_organization membership row.
        if (!hasGlobalAccess
            && organizations.Count == 0
            && user.OrganizationId.HasValue
            && user.Organization is not null)
        {
            organizations =
            [
                new UserOrgDto
                {
                    OrganizationId = user.OrganizationId.Value,
                    OrganizationName = user.Organization.ShortName,
                    RoleId = user.RoleId,
                    RoleName = user.Role.FullName,
                    IsDefault = true
                }
            ];

            _logger.LogWarning(
                "Authentication used legacy sys_user.organization_id fallback for {UserName}. Consider backfilling sys_user_organization for user {UserId}.",
                normalizedUserName,
                user.Id);
        }

        if (!hasGlobalAccess && organizations.Count == 0)
        {
            _logger.LogInformation(
                "Authentication failed for {UserName}: non-global user {UserId} has no active organization memberships.",
                normalizedUserName,
                user.Id);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

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

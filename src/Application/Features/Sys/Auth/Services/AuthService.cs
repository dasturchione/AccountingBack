using Domain.Entities;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using SharedKernel.Constants;
using SharedKernel.Query;
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

    public AuthService(
        IUserContext userContext,
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

    public ValueTask<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default) =>
        AuthenticateAsync(dto, requireGlobalAccess: false, ct);

    public ValueTask<Result<LoginResponseDto>> SuperAdminLoginAsync(LoginDto dto, CancellationToken ct = default) =>
        AuthenticateAsync(dto, requireGlobalAccess: true, ct);

    private async ValueTask<Result<LoginResponseDto>> AuthenticateAsync(
        LoginDto dto,
        bool requireGlobalAccess,
        CancellationToken ct = default)
    {
        var normalizedUserName = dto.UserName.Trim();

        var query = _queryBuilder.For<User>()
            .Where(x => x.UserName == normalizedUserName)
            .IgnoreQueryFilters()
            .AddIncludes(builder =>
            {
                builder.Include(user => user.State);
                builder.Include(user => user.UserKind);
            })
            .Build();

        var user = await _userQuery.GetAsync(query, ct);
        if (user is null || user.State is null || user.StateId != StateIdConst.ACTIVE)
        {
            _logger.LogInformation(
                "Authentication failed for {UserName}: user not found, inactive, or missing state.",
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

        var isSuperAdmin = user.UserKindId == UserKindIdConst.SuperAdmin;
        if (isSuperAdmin != requireGlobalAccess)
        {
            _logger.LogInformation(
                "Authentication failed for {UserName}: user kind {UserKindId} does not match endpoint global-access requirement {RequireGlobalAccess}.",
                normalizedUserName,
                user.UserKindId,
                requireGlobalAccess);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

        var organizationSpec = _queryBuilder.For<UserOrganization>()
            .Where(membership => membership.UserId == user.Id && membership.StateId == StateIdConst.ACTIVE)
            .IgnoreQueryFilters()
            .As(membership => new UserOrgDto
            {
                OrganizationId = membership.OrganizationId,
                OrganizationName = membership.Organization.ShortName,
                RoleId = membership.RoleId,
                RoleName = membership.Role != null ? membership.Role.FullName : null,
                IsDefault = membership.IsDefault
            })
            .Build();

        var organizations = await _userOrgQuery.GetAllAsync(organizationSpec, ct);
        if (!isSuperAdmin && organizations.Count == 0)
        {
            _logger.LogInformation(
                "Authentication failed for {UserName}: non-global user {UserId} has no active organization memberships.",
                normalizedUserName,
                user.Id);
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials(_userContext.LanguageId));
        }

        var defaultOrganization = organizations.FirstOrDefault(organization => organization.IsDefault)
            ?? organizations.FirstOrDefault();
        var token = _tokenProvider.GenerateAccessToken(user, defaultOrganization?.OrganizationId ?? 0);
        var permissions = isSuperAdmin
            ? await GetAllActivePermissionCodesAsync(ct)
            : defaultOrganization?.RoleId is int roleId
                ? await GetRolePermissionCodesAsync(roleId, ct)
                : [];

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
                TenantId = user.TenantId,
                UserKindId = user.UserKindId,
                UserKindCode = user.UserKind.Code,
                StateName = user.State.FullName,
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
        var permissionSpec = _queryBuilder.For<RoleModule>()
            .Where(roleModule => roleModule.RoleId == roleId)
            .As(roleModule => roleModule.Module.Code)
            .Build();

        return (await _roleModuleQuery.GetAllAsync(permissionSpec, ct)).ToList();
    }

    private async Task<List<string>> GetAllActivePermissionCodesAsync(CancellationToken ct)
    {
        var permissionSpec = _queryBuilder.For<Module>()
            .Where(module => module.StateId == StateIdConst.ACTIVE)
            .As(module => module.Code)
            .Build();

        return (await _moduleQuery.GetAllAsync(permissionSpec, ct)).ToList();
    }
}

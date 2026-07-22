using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Features.Platform;
using Application.Features.Users;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Users.Services;

public sealed class UserManagementCore : IUserManagementCore
{
    private readonly IUserContext _userContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IQueryRepository<User> _userQuery;
    private readonly ICommandRepository<User> _userCommand;
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly ICommandRepository<UserOrganization> _userOrganizationCommand;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserManagementCore> _logger;

    public UserManagementCore(
        IUserContext userContext,
        IPasswordHasher passwordHasher,
        IQueryRepository<User> userQuery,
        ICommandRepository<User> userCommand,
        IQueryRepository<UserOrganization> userOrganizationQuery,
        ICommandRepository<UserOrganization> userOrganizationCommand,
        IQueryRepository<Role> roleQuery,
        IQueryRepository<Organization> organizationQuery,
        IEmailSender emailSender,
        ILogger<UserManagementCore> logger)
    {
        _userContext = userContext;
        _passwordHasher = passwordHasher;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userOrganizationQuery = userOrganizationQuery;
        _userOrganizationCommand = userOrganizationCommand;
        _roleQuery = roleQuery;
        _organizationQuery = organizationQuery;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result<UserManagementCreateResult>> CreateUserAsync(
        UserManagementCreateRequest request,
        UserManagementOptions options,
        CancellationToken ct = default)
    {
        if (options.Scope == UserManagementScope.Global && !_userContext.HasGlobalAccess)
            return Result.Failure<UserManagementCreateResult>(PlatformErrors.GlobalAccessRequired());

        var prepared = PrepareCreateRequest(request, options.Scope);
        var exists = await _userQuery.AnyAsync(x => x.UserName == prepared.UserName, ct);
        if (exists)
            return Result.Failure<UserManagementCreateResult>(ResolveUserNameConflict(prepared.UserName, options.Scope));

        List<UserManagementMembershipRequest> memberships;
        if (options.Scope == UserManagementScope.Global)
        {
            var roleValidation = await ValidateGlobalRoleAsync(prepared.RoleId, ct);
            if (roleValidation is not null)
                return Result.Failure<UserManagementCreateResult>(roleValidation);

            memberships = NormalizeGlobalMemberships(prepared.Organizations);
            var membershipValidation = await ValidateGlobalMembershipsAsync(memberships, ct);
            if (membershipValidation is not null)
                return Result.Failure<UserManagementCreateResult>(membershipValidation);
        }
        else
        {
            memberships = DistinctOrganizationMemberships(prepared.Organizations);
        }

        var now = DateTime.Now;
        var salt = _passwordHasher.GenerateSalt();
        var user = new User
        {
            UserName = prepared.UserName,
            PhoneNumber = prepared.PhoneNumber,
            Email = prepared.Email,
            FirstName = prepared.FirstName,
            LastName = prepared.LastName,
            RoleId = prepared.RoleId,
            TenantId = request.TenantId,
            LanguageId = options.Scope == UserManagementScope.Global ? prepared.LanguageId : null,
            EmailVerified = prepared.EmailVerified,
            EmailVerifiedAt = options.Scope == UserManagementScope.Global && prepared.EmailVerified ? now : null,
            IsPlatformAdmin = prepared.IsPlatformAdmin,
            Timezone = prepared.Timezone,
            PasswordSalt = salt,
            PasswordHash = _passwordHasher.Hash(prepared.Password, salt),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = now
        };

        await _userCommand.CreateAsync(user, ct);

        if (options.Scope == UserManagementScope.Global)
            await SyncGlobalMembershipsAsync(user.Id, memberships, replaceExisting: false, ct);
        else
            await CreateOrganizationMembershipsAsync(user.Id, memberships, includeJoinedAt: true, ct);

        return Result.Success(new UserManagementCreateResult
        {
            UserId = user.Id,
            WelcomeEmail = options.SendWelcomeEmail && !string.IsNullOrWhiteSpace(prepared.Email)
                ? new UserWelcomeEmailMessage
                {
                    Email = prepared.Email!,
                    UserName = prepared.UserName,
                    FirstName = prepared.FirstName,
                    LastName = prepared.LastName
                }
                : null
        });
    }

    public async Task<Result> UpdateUserAsync(
        UserManagementUpdateRequest request,
        UserManagementOptions options,
        CancellationToken ct = default)
    {
        if (options.Scope == UserManagementScope.Global && !_userContext.HasGlobalAccess)
            return Result.Failure(PlatformErrors.GlobalAccessRequired());

        var user = await _userQuery.GetAsync(new QuerySpecification<User> { Criteria = x => x.Id == request.UserId }, ct);
        if (user is null)
            return Result.Failure(ResolveUserNotFound(request.UserId, options.Scope));

        var prepared = PrepareUpdateRequest(request, options.Scope);
        if (ShouldCheckUserNameConflict(user.UserName, prepared.UserName, options.Scope))
        {
            var exists = await _userQuery.AnyAsync(x => x.Id != request.UserId && x.UserName == prepared.UserName, ct);
            if (exists)
                return Result.Failure(ResolveUserNameConflict(prepared.UserName, options.Scope));
        }

        List<UserManagementMembershipRequest>? memberships = null;
        if (options.Scope == UserManagementScope.Global)
        {
            var roleValidation = await ValidateGlobalRoleAsync(prepared.RoleId, ct);
            if (roleValidation is not null)
                return Result.Failure(roleValidation);

            if (prepared.Organizations is not null)
            {
                memberships = NormalizeGlobalMemberships(prepared.Organizations);
                var membershipValidation = await ValidateGlobalMembershipsAsync(memberships, ct);
                if (membershipValidation is not null)
                    return Result.Failure(membershipValidation);
            }
        }
        else
        {
            memberships = DistinctOrganizationMemberships(prepared.Organizations ?? []);
        }

        var now = DateTime.Now;
        user.UserName = prepared.UserName;
        user.PhoneNumber = prepared.PhoneNumber;
        user.Email = prepared.Email;
        user.FirstName = prepared.FirstName;
        user.LastName = prepared.LastName;
        user.RoleId = prepared.RoleId;
        user.EmailVerified = prepared.EmailVerified;
        user.IsPlatformAdmin = prepared.IsPlatformAdmin;
        user.Timezone = prepared.Timezone;
        user.StateId = prepared.StateId;

        if (options.Scope == UserManagementScope.Global)
        {
            user.LanguageId = prepared.LanguageId;
            user.EmailVerifiedAt = prepared.EmailVerified ? user.EmailVerifiedAt ?? now : null;
        }

        await _userCommand.UpdateAsync(user, ct);

        if (options.Scope == UserManagementScope.Global)
        {
            if (memberships is not null)
                await SyncGlobalMembershipsAsync(user.Id, memberships, replaceExisting: true, ct);
        }
        else
        {
            await _userOrganizationCommand.DeleteAsync(x => x.UserId == user.Id, ct);
            await CreateOrganizationMembershipsAsync(user.Id, memberships ?? [], includeJoinedAt: false, ct);
        }

        return Result.Success();
    }

    public async Task SendWelcomeEmailSafeAsync(UserWelcomeEmailMessage message, CancellationToken ct = default)
    {
        try
        {
            var fullName = $"{message.FirstName} {message.LastName}".Trim();
            var greeting = string.IsNullOrWhiteSpace(fullName) ? message.UserName : fullName;

            var emailResult = await _emailSender.SendAsync(new EmailMessage
            {
                To = [message.Email],
                Subject = "Accounting ERP - akkountingiz yaratildi",
                HtmlBody = BuildWelcomeHtml(greeting, message.UserName)
            }, ct);

            if (!emailResult.IsSuccess)
                _logger.LogWarning("Welcome email yuborilmadi ({Email}): {Error}", message.Email, emailResult.Error.Description);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Welcome email yuborishda kutilmagan xato ({Email})", message.Email);
        }
    }

    private async Task<Error?> ValidateGlobalRoleAsync(int roleId, CancellationToken ct)
    {
        var exists = await _roleQuery.AnyAsync(x => x.Id == roleId && x.StateId == StateIdConst.ACTIVE, ct);
        return exists ? null : PlatformErrors.RoleNotFound(roleId);
    }

    private async Task<Error?> ValidateGlobalMembershipsAsync(List<UserManagementMembershipRequest> memberships, CancellationToken ct)
    {
        foreach (var membership in memberships)
        {
            var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == membership.OrganizationId, ct);
            if (!organizationExists)
                return PlatformErrors.OrganizationNotFound(membership.OrganizationId);

            if (membership.RoleId.HasValue)
            {
                var roleValidation = await ValidateGlobalRoleAsync(membership.RoleId.Value, ct);
                if (roleValidation is not null)
                    return roleValidation;
            }

            if (membership.InvitedByUserId.HasValue)
            {
                var userExists = await _userQuery.AnyAsync(x => x.Id == membership.InvitedByUserId.Value, ct);
                if (!userExists)
                    return PlatformErrors.UserNotFound(membership.InvitedByUserId.Value);
            }
        }

        return null;
    }

    private async Task CreateOrganizationMembershipsAsync(
        int userId,
        List<UserManagementMembershipRequest> memberships,
        bool includeJoinedAt,
        CancellationToken ct)
    {
        if (memberships.Count == 0)
            return;

        var now = DateTime.Now;
        var entities = memberships
            .Select((membership, index) => new UserOrganization
            {
                UserId = userId,
                OrganizationId = membership.OrganizationId,
                IsDefault = index == 0,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                JoinedAt = includeJoinedAt ? now : default
            })
            .ToList();

        await _userOrganizationCommand.CreateAsync(entities, ct);
    }

    private async Task SyncGlobalMembershipsAsync(
        int userId,
        List<UserManagementMembershipRequest> memberships,
        bool replaceExisting,
        CancellationToken ct)
    {
        var now = DateTime.Now;
        var existing = await _userOrganizationQuery.GetAllAsync(new QuerySpecification<UserOrganization>
        {
            Criteria = x => x.UserId == userId
        }, ct);

        var requestedOrganizationIds = memberships.Select(x => x.OrganizationId).ToHashSet();
        var toUpdate = new List<UserOrganization>();
        var toCreate = new List<UserOrganization>();

        if (replaceExisting)
        {
            foreach (var membership in existing.Where(x => !requestedOrganizationIds.Contains(x.OrganizationId)))
            {
                membership.IsDefault = false;
                membership.StateId = StateIdConst.PASSIVE;
                membership.BlockedAt ??= now;
                toUpdate.Add(membership);
            }
        }

        foreach (var dto in memberships)
        {
            var membership = existing.FirstOrDefault(x => x.OrganizationId == dto.OrganizationId);
            if (membership is null)
            {
                membership = new UserOrganization
                {
                    UserId = userId,
                    OrganizationId = dto.OrganizationId,
                    CreatedDate = now,
                    JoinedAt = now
                };
                ApplyGlobalMembership(membership, dto, now);
                toCreate.Add(membership);
            }
            else
            {
                ApplyGlobalMembership(membership, dto, now);
                toUpdate.Add(membership);
            }
        }

        if (toCreate.Count > 0)
            await _userOrganizationCommand.CreateAsync(toCreate, ct);

        if (toUpdate.Count > 0)
            await _userOrganizationCommand.UpdateAsync(toUpdate, ct);
    }

    private static void ApplyGlobalMembership(UserOrganization membership, UserManagementMembershipRequest request, DateTime now)
    {
        membership.RoleId = request.RoleId;
        membership.IsDefault = request.IsDefault;
        membership.IsOwner = request.IsOwner;
        membership.InvitedByUserId = request.InvitedByUserId;
        membership.StateId = StateIdConst.ACTIVE;
        membership.BlockedAt = null;
        if (membership.JoinedAt == default)
            membership.JoinedAt = now;
    }

    private static UserManagementCreateRequest PrepareCreateRequest(UserManagementCreateRequest request, UserManagementScope scope) =>
        scope == UserManagementScope.Global
            ? new UserManagementCreateRequest
            {
                UserName = request.UserName.Trim(),
                Password = request.Password,
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                RoleId = request.RoleId,
                LanguageId = request.LanguageId,
                EmailVerified = request.EmailVerified,
                IsPlatformAdmin = request.IsPlatformAdmin,
                Timezone = request.Timezone,
                Organizations = request.Organizations
            }
            : request;

    private static UserManagementUpdateRequest PrepareUpdateRequest(UserManagementUpdateRequest request, UserManagementScope scope) =>
        scope == UserManagementScope.Global
            ? new UserManagementUpdateRequest
            {
                UserId = request.UserId,
                UserName = request.UserName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                RoleId = request.RoleId,
                LanguageId = request.LanguageId,
                EmailVerified = request.EmailVerified,
                IsPlatformAdmin = request.IsPlatformAdmin,
                Timezone = request.Timezone,
                StateId = request.StateId,
                Organizations = request.Organizations
            }
            : request;

    private static bool ShouldCheckUserNameConflict(string currentUserName, string requestedUserName, UserManagementScope scope) =>
        scope == UserManagementScope.Global
            ? !string.Equals(currentUserName, requestedUserName, StringComparison.OrdinalIgnoreCase)
            : currentUserName != requestedUserName;

    private static List<UserManagementMembershipRequest> DistinctOrganizationMemberships(IEnumerable<UserManagementMembershipRequest> memberships) =>
        memberships
            .Where(x => x.OrganizationId > 0)
            .GroupBy(x => x.OrganizationId)
            .Select(x => x.First())
            .Select(x => new UserManagementMembershipRequest
            {
                OrganizationId = x.OrganizationId
            })
            .ToList();

    private static List<UserManagementMembershipRequest> NormalizeGlobalMemberships(IEnumerable<UserManagementMembershipRequest> memberships)
    {
        var normalized = memberships
            .Where(x => x.OrganizationId > 0)
            .GroupBy(x => x.OrganizationId)
            .Select(x => x.First())
            .Select(x => new UserManagementMembershipRequest
            {
                OrganizationId = x.OrganizationId,
                RoleId = x.RoleId,
                IsDefault = x.IsDefault,
                IsOwner = x.IsOwner,
                InvitedByUserId = x.InvitedByUserId
            })
            .ToList();

        if (normalized.Count == 0)
            return normalized;

        var defaultAssigned = false;
        for (var index = 0; index < normalized.Count; index++)
        {
            if (!defaultAssigned && normalized[index].IsDefault)
            {
                defaultAssigned = true;
                continue;
            }

            normalized[index] = new UserManagementMembershipRequest
            {
                OrganizationId = normalized[index].OrganizationId,
                RoleId = normalized[index].RoleId,
                IsDefault = false,
                IsOwner = normalized[index].IsOwner,
                InvitedByUserId = normalized[index].InvitedByUserId
            };
        }

        if (!defaultAssigned)
        {
            normalized[0] = new UserManagementMembershipRequest
            {
                OrganizationId = normalized[0].OrganizationId,
                RoleId = normalized[0].RoleId,
                IsDefault = true,
                IsOwner = normalized[0].IsOwner,
                InvitedByUserId = normalized[0].InvitedByUserId
            };
        }

        return normalized;
    }

    private static int? GetDefaultOrganizationId(List<UserManagementMembershipRequest> memberships) =>
        memberships.FirstOrDefault(x => x.IsDefault)?.OrganizationId
        ?? memberships.FirstOrDefault()?.OrganizationId;

    private Error ResolveUserNameConflict(string userName, UserManagementScope scope) =>
        scope == UserManagementScope.Global
            ? PlatformErrors.UserNameConflict(userName)
            : UserErrors.Conflict(userName, _userContext.LanguageId);

    private Error ResolveUserNotFound(int userId, UserManagementScope scope) =>
        scope == UserManagementScope.Global
            ? PlatformErrors.UserNotFound(userId)
            : UserErrors.NotFound(userId, _userContext.LanguageId);

    private static string BuildWelcomeHtml(string greeting, string userName) =>
        $"""
        <div style="font-family:Arial,sans-serif;max-width:520px;margin:auto;color:#222">
          <h2 style="color:#1a73e8;margin-bottom:4px">Accounting ERP</h2>
          <p>Assalomu alaykum, <b>{greeting}</b>!</p>
          <p>Sizning hisobingiz muvaffaqiyatli yaratildi.</p>
          <p style="background:#f5f5f5;padding:10px 14px;border-radius:6px">
            Login (foydalanuvchi nomi): <b>{userName}</b>
          </p>
          <p>Parolni administratoringizdan oling va tizimga kiring.</p>
          <hr style="border:none;border-top:1px solid #eee;margin:18px 0"/>
          <p style="font-size:12px;color:#888">Bu avtomatik xabar - javob yozmang.</p>
        </div>
        """;
}

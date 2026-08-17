using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Features.Platform;
using Application.Features.Users;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
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
    private readonly IQueryRepository<UserKind> _userKindQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserManagementCore> _logger;
    private readonly IQueryBuilder _queryBuilder;

    public UserManagementCore(
        IUserContext userContext,
        IPasswordHasher passwordHasher,
        IQueryRepository<User> userQuery,
        ICommandRepository<User> userCommand,
        IQueryRepository<UserOrganization> userOrganizationQuery,
        ICommandRepository<UserOrganization> userOrganizationCommand,
        IQueryRepository<Role> roleQuery,
        IQueryRepository<UserKind> userKindQuery,
        IQueryRepository<Organization> organizationQuery,
        IEmailSender emailSender,
        IQueryBuilder queryBuilder,
        ILogger<UserManagementCore> logger)
    {
        _userContext = userContext;
        _passwordHasher = passwordHasher;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _userOrganizationQuery = userOrganizationQuery;
        _userOrganizationCommand = userOrganizationCommand;
        _roleQuery = roleQuery;
        _userKindQuery = userKindQuery;
        _organizationQuery = organizationQuery;
        _emailSender = emailSender;
        _queryBuilder = queryBuilder;
        _logger = logger;
    }

    public async Task<Result<UserManagementCreateResult>> CreateUserAsync(
        UserManagementCreateRequest request,
        UserManagementOptions options,
        CancellationToken ct = default)
    {
        if (options.Scope == UserManagementScope.Global && _userContext.UserKind != CurrentUserKind.SuperAdmin)
            return Result.Failure<UserManagementCreateResult>(PlatformErrors.GlobalAccessRequired());

        var prepared = PrepareCreateRequest(request, options.Scope);
        var exists = await _userQuery.AnyAsync(user => user.UserName == prepared.UserName, ct);
        if (exists)
            return Result.Failure<UserManagementCreateResult>(ResolveUserNameConflict(prepared.UserName, options.Scope));

        var userKindValidation = await ValidateUserKindAsync(prepared.UserKindId, ct);
        if (userKindValidation is not null)
            return Result.Failure<UserManagementCreateResult>(userKindValidation);

        var memberships = NormalizeMemberships(prepared.Organizations);
        var membershipValidation = await ValidateMembershipsAsync(memberships, ct);
        if (membershipValidation is not null)
            return Result.Failure<UserManagementCreateResult>(membershipValidation);

        var now = DateTime.Now;
        var salt = _passwordHasher.GenerateSalt();
        var user = new User
        {
            UserName = prepared.UserName,
            PhoneNumber = prepared.PhoneNumber,
            Email = prepared.Email,
            FirstName = prepared.FirstName,
            LastName = prepared.LastName,
            TenantId = prepared.TenantId,
            UserKindId = prepared.UserKindId,
            LanguageId = options.Scope == UserManagementScope.Global ? prepared.LanguageId : null,
            EmailVerified = prepared.EmailVerified,
            EmailVerifiedAt = options.Scope == UserManagementScope.Global && prepared.EmailVerified ? now : null,
            Timezone = prepared.Timezone,
            PasswordSalt = salt,
            PasswordHash = _passwordHasher.Hash(prepared.Password, salt),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = now
        };

        await _userCommand.CreateAsync(user, ct);
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
        if (options.Scope == UserManagementScope.Global && _userContext.UserKind != CurrentUserKind.SuperAdmin)
            return Result.Failure(PlatformErrors.GlobalAccessRequired());

        var user = await _userQuery.GetAsync(_queryBuilder.For<User>().Where(item => item.Id == request.UserId).Build(), ct);
        if (user is null)
            return Result.Failure(ResolveUserNotFound(request.UserId, options.Scope));

        var prepared = PrepareUpdateRequest(request, options.Scope);
        if (ShouldCheckUserNameConflict(user.UserName, prepared.UserName, options.Scope))
        {
            var exists = await _userQuery.AnyAsync(item => item.Id != request.UserId && item.UserName == prepared.UserName, ct);
            if (exists)
                return Result.Failure(ResolveUserNameConflict(prepared.UserName, options.Scope));
        }

        List<UserManagementMembershipRequest>? memberships = null;
        if (options.Scope == UserManagementScope.Global)
        {
            var userKindValidation = await ValidateUserKindAsync(prepared.UserKindId, ct);
            if (userKindValidation is not null)
                return Result.Failure(userKindValidation);

            if (prepared.Organizations is not null)
            {
                memberships = NormalizeMemberships(prepared.Organizations);
                var membershipValidation = await ValidateMembershipsAsync(memberships, ct);
                if (membershipValidation is not null)
                    return Result.Failure(membershipValidation);
            }
        }
        else
        {
            memberships = NormalizeMemberships(prepared.Organizations ?? []);
            var membershipValidation = await ValidateMembershipsAsync(memberships, ct);
            if (membershipValidation is not null)
                return Result.Failure(membershipValidation);
        }

        var now = DateTime.Now;
        user.UserName = prepared.UserName;
        user.PhoneNumber = prepared.PhoneNumber;
        user.Email = prepared.Email;
        user.FirstName = prepared.FirstName;
        user.LastName = prepared.LastName;
        user.EmailVerified = prepared.EmailVerified;
        user.Timezone = prepared.Timezone;
        user.StateId = prepared.StateId;

        if (options.Scope == UserManagementScope.Global)
        {
            user.UserKindId = prepared.UserKindId;
            user.LanguageId = prepared.LanguageId;
            user.EmailVerifiedAt = prepared.EmailVerified ? user.EmailVerifiedAt ?? now : null;
        }

        await _userCommand.UpdateAsync(user, ct);

        if (options.Scope == UserManagementScope.Global)
        {
            if (memberships is not null)
                await SyncMembershipsAsync(user.Id, memberships, replaceExisting: true, ct);
        }
        else
        {
            await _userOrganizationCommand.DeleteAsync(membership => membership.UserId == user.Id, ct);
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

    private async Task<Error?> ValidateUserKindAsync(short userKindId, CancellationToken ct)
    {
        var exists = await _userKindQuery.AnyAsync(kind => kind.Id == userKindId, ct);
        return exists ? null : PlatformErrors.UserKindNotFound(userKindId);
    }

    private async Task<Error?> ValidateRoleAsync(int roleId, CancellationToken ct)
    {
        var exists = await _roleQuery.AnyAsync(role => role.Id == roleId && role.StateId == StateIdConst.ACTIVE, ct);
        return exists ? null : PlatformErrors.RoleNotFound(roleId);
    }

    private async Task<Error?> ValidateMembershipsAsync(List<UserManagementMembershipRequest> memberships, CancellationToken ct)
    {
        foreach (var membership in memberships)
        {
            var organizationExists = await _organizationQuery.AnyAsync(organization => organization.Id == membership.OrganizationId, ct);
            if (!organizationExists)
                return PlatformErrors.OrganizationNotFound(membership.OrganizationId);

            if (membership.RoleId.HasValue)
            {
                var roleValidation = await ValidateRoleAsync(membership.RoleId.Value, ct);
                if (roleValidation is not null)
                    return roleValidation;
            }

            if (membership.InvitedByUserId.HasValue)
            {
                var userExists = await _userQuery.AnyAsync(user => user.Id == membership.InvitedByUserId.Value, ct);
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
        var entities = memberships.Select(membership =>
        {
            var entity = new UserOrganization
            {
                UserId = userId,
                OrganizationId = membership.OrganizationId,
                CreatedDate = now,
                JoinedAt = includeJoinedAt ? now : default
            };
            ApplyMembership(entity, membership, now);
            return entity;
        }).ToList();

        await _userOrganizationCommand.CreateAsync(entities, ct);
    }

    private async Task SyncMembershipsAsync(
        int userId,
        List<UserManagementMembershipRequest> memberships,
        bool replaceExisting,
        CancellationToken ct)
    {
        var now = DateTime.Now;
        var existing = await _userOrganizationQuery.GetAllAsync(_queryBuilder.For<UserOrganization>()
            .Where(membership => membership.UserId == userId)
            .Build(), ct);

        var requestedOrganizationIds = memberships.Select(membership => membership.OrganizationId).ToHashSet();
        var toUpdate = new List<UserOrganization>();
        var toCreate = new List<UserOrganization>();

        if (replaceExisting)
        {
            foreach (var membership in existing.Where(item => !requestedOrganizationIds.Contains(item.OrganizationId)))
            {
                membership.IsDefault = false;
                membership.StateId = StateIdConst.PASSIVE;
                membership.BlockedAt ??= now;
                toUpdate.Add(membership);
            }
        }

        foreach (var request in memberships)
        {
            var membership = existing.FirstOrDefault(item => item.OrganizationId == request.OrganizationId);
            if (membership is null)
            {
                membership = new UserOrganization
                {
                    UserId = userId,
                    OrganizationId = request.OrganizationId,
                    CreatedDate = now,
                    JoinedAt = now
                };
                ApplyMembership(membership, request, now);
                toCreate.Add(membership);
            }
            else
            {
                ApplyMembership(membership, request, now);
                toUpdate.Add(membership);
            }
        }

        if (toCreate.Count > 0)
            await _userOrganizationCommand.CreateAsync(toCreate, ct);

        if (toUpdate.Count > 0)
            await _userOrganizationCommand.UpdateAsync(toUpdate, ct);
    }

    private static void ApplyMembership(UserOrganization membership, UserManagementMembershipRequest request, DateTime now)
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
                TenantId = request.TenantId,
                UserKindId = request.UserKindId,
                LanguageId = request.LanguageId,
                EmailVerified = request.EmailVerified,
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
                UserKindId = request.UserKindId,
                LanguageId = request.LanguageId,
                EmailVerified = request.EmailVerified,
                Timezone = request.Timezone,
                StateId = request.StateId,
                Organizations = request.Organizations
            }
            : request;

    private static bool ShouldCheckUserNameConflict(string currentUserName, string requestedUserName, UserManagementScope scope) =>
        scope == UserManagementScope.Global
            ? !string.Equals(currentUserName, requestedUserName, StringComparison.OrdinalIgnoreCase)
            : currentUserName != requestedUserName;

    private static List<UserManagementMembershipRequest> NormalizeMemberships(IEnumerable<UserManagementMembershipRequest> memberships)
    {
        var normalized = memberships
            .Where(membership => membership.OrganizationId > 0)
            .GroupBy(membership => membership.OrganizationId)
            .Select(group => group.First())
            .Select(membership => new UserManagementMembershipRequest
            {
                OrganizationId = membership.OrganizationId,
                RoleId = membership.RoleId,
                IsDefault = membership.IsDefault,
                IsOwner = membership.IsOwner,
                InvitedByUserId = membership.InvitedByUserId
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
